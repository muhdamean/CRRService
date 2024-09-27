# CRRService
C# Service for Sending Data to an external API.


## What's Available:
1. Developed using .NET 8
2. Uses the `HttpClient` class for sending HTTP requests
3. Supports both synchronous and asynchronous methods for sending data
4. Use Dapper for communicating with database.

## How to:
# Run: 
      dotnet run
# Publish: 
      dotnet publish

## Service Installation and Management:
1. sc create ServiceName binPath= "C:\{path to file}\CrrService.exe"
2. Start the service:               sc start ServiceName
3. Check the status of the service: sc query ServiceName
4. Stop the service:                sc stop ServiceName
5. Delete the service:              sc delete ServiceName

## Questions

If you have questions please feel free to reach out to mailcontact2016@gmail.com
