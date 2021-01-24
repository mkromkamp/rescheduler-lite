﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Rescheduler.Infra.Data;

namespace Rescheduler.Api.Migrations
{
    [DbContext(typeof(JobContext))]
    partial class JobContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("Rescheduler.Core.Entities.Job", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Cron")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Enabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("RunAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StopAfter")
                        .HasColumnType("TEXT");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Jobs");
                });

            modelBuilder.Entity("Rescheduler.Core.Entities.ScheduledJob", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("JobId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("QueuedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ScheduledAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("JobId");

                    b.HasIndex("QueuedAt", "Status");

                    b.ToTable("ScheduledJobs");
                });

            modelBuilder.Entity("Rescheduler.Core.Entities.ScheduledJob", b =>
                {
                    b.HasOne("Rescheduler.Core.Entities.Job", "Job")
                        .WithMany()
                        .HasForeignKey("JobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Job");
                });
#pragma warning restore 612, 618
        }
    }
}