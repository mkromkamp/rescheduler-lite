using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Rescheduler.Core.Entities;
using Rescheduler.Core.Interfaces;
using Rescheduler.Infra.Metrics;

namespace Rescheduler.Infra.Messaging;

internal class RabbitJobPublisher : IJobPublisher
{
    private readonly ILogger<RabbitJobPublisher> _logger;
    private readonly IConnectionFactory _connectionFactory;
    private readonly IMessagingMetrics _metrics;
    private RabbitMqOptions _options;
    private IModel? _model;

    public RabbitJobPublisher(IConnectionFactory connectionFactory, ILogger<RabbitJobPublisher> logger, IOptionsMonitor<MessagingOptions> optionsMonitor, IMessagingMetrics metrics)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _metrics = metrics;

        _options = optionsMonitor.CurrentValue.RabbitMq;
        optionsMonitor.OnChange(newOptions => 
        {
            if (newOptions?.RabbitMq is null) return;
                
            _options = newOptions.RabbitMq;
        });
    }

    public Task<bool> PublishAsync(JobExecution jobExecution, CancellationToken ctx)
    {
        var t = Stopwatch.StartNew();

        try
        {
            if (!TryGetOrCreateModel(out var model) || model is null) return Task.FromResult(false);
            var job = jobExecution.Job;

            model.EnsureConfig(_options.JobsExchange, job.Subject);
            model.BasicPublish(_options.JobsExchange, job.Subject, true, null, Encoding.UTF8.GetBytes(job.Payload));

            _metrics.MessagesPublished(job.Subject);

            return Task.FromResult(model.WaitForConfirms());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to publish job {JobId} to RabbitMQ", jobExecution.Job.Id);
            return Task.FromResult(false);
        }
        finally
        {
            _metrics.TimePublishDuration(t.Elapsed);
        }
    }

    public Task<bool> PublishManyAsync(IEnumerable<JobExecution> jobExecutions, CancellationToken ctx)
    {
        var t = Stopwatch.StartNew();
        jobExecutions = jobExecutions.ToList();

        try
        {
            if(!TryGetOrCreateModel(out var model) || model is null) return Task.FromResult(false);
            var batchPublish = model.CreateBasicPublishBatch();
                
            jobExecutions.GroupBy(j => j.Job.Subject).ToList().ForEach(g => 
            {
                model.EnsureConfig(_options.JobsExchange, g.Key);
                foreach (var jobExecution in g.ToList())
                {
                    batchPublish.Add(_options.JobsExchange, jobExecution.Job.Subject, true, null, new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(jobExecution.Job.Payload)));
                }

                _metrics.MessagesPublished(g.Key, g.Count());
            });

            batchPublish.Publish();
            return Task.FromResult(model.WaitForConfirms());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to batch publish {JobIds} to RabbitMQ", jobExecutions.Select(j => j.Job.Id));
            return Task.FromResult(false);
        }
        finally
        {
            _metrics.TimePublishBatchDuration(t.Elapsed);
        }
    }

    private bool TryGetOrCreateModel(out IModel? model)
    {
        if (_model is null || _model.IsClosed)
        {
            try
            {   
                _model = _connectionFactory.CreateConnection().CreateModel();
                _model.ConfirmSelect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create RabbitMQ connection");

                model = null;
                return false;
            }
        }

        model = _model;
        return true;
    }
}