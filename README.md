![.NET](https://github.com/mkromkamp/rescheduler-lite/workflows/.NET/badge.svg)
# Rescheduler-lite

Rescheduler-lite is a job/work scheduler that is easy to deploy, operate, and worker/environment agnostic

## Features

- Single binary
- Low memory footprint
- Built-in observability
- Build-in backup and restore
- Option for retry mechanisms
- Build on top of standard components
- Leverage [NSQ](https://nsq.io/) for message passing

## Goals

The main goal of rescheduler-lite is to simplify scheduled/recurring jobs for small and mid-size projects. 

More often than not a basic feature needs to be implemented that requires *things* to happen in the future (sending reminder emails, webhooks, etc.) or to perform a recurring task (generating reports, collecting external information, etc..). 

Where these *things* are usually really project specific the scheduling of these jobs is not. Still it can be a hassle to get job scheduling setup in a convenient and low maintenance fashion.

rescheduler-lite aims to take away this generic scheduling of jobs while not having any opinions around the way these jobs are implemented or executed. All of this while being easy to deploy and maintain for smaller teams or companies.

## High level architecture

[TODO]

## Deployment

rescheduler-lite consists of a single binary that can be deployed virtually everywhere. The only constraint is the availability of some form of persistent storage.

Alongside rescheduler-lite it is needed to deploy a single nsq daemon. Nsq is used to pass messages from rescheduler-lite to consumers

### Binary

Describe binary deployment

### Docker

Describe binary deployment
