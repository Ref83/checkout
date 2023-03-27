#!/bin/bash
set -e

docker-compose up -d

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# build and run acquiring bank
osascript -e "tell application \"Terminal\" to do script \"
  cd $__dir && 
  dotnet build Interview.Bank/Interview.Bank.sln && 
  dotnet run --project Interview.Bank/Interview.Bank.Host/Interview.Bank.Host.csproj\""

# build, test and run payments executor
osascript -e "tell application \"Terminal\" to do script \"
  cd $__dir && 
  dotnet build Interview.PaymentExecutor/Interview.PaymentExecutor.sln && 
  dotnet test Interview.PaymentExecutor/Interview.PaymentExecutor.sln &&
  dotnet run --project Interview.PaymentExecutor/Interview.PaymentExecutor.Host/Interview.PaymentExecutor.Host.csproj\""

# build, test and run payments gateway
osascript -e "tell application \"Terminal\" to do script \"
  cd $__dir && 
  dotnet build Interview.PaymentGateway/Interview.PaymentGateway.sln && 
  dotnet test Interview.PaymentGateway/Interview.PaymentGateway.sln &&
  dotnet run --project Interview.PaymentGateway/Interview.PaymentGateway.Host/Interview.PaymentGateway.Host.csproj\""



