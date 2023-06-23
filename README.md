# DataFactoryAutomation

This application shows how to use the Azure .NET library to create and execute Azure Data Factory from C# code. This demonstration project shows how to create pipelines that copy data from one source, with the two data sources being SQL Server and Salesforce.

## Running the application

In order to run the application you will need to replace the `#{TOKEN}#` values assocated with the properties under `ConnectionStrings`, `Azure`, and `Salesforce` in **appsettings.json**. These sections define how the application will connect with the various data sources and services. Unfortunately we don't have a way to set up a safe sandbox for you to play in easily.

Upon initialization the application will create the necessary SQL tables based on the schema and data defined in the **WebApi\Data\Database** folder.

## Resources

The presentation that goes with this project that was presented at the [Philadelphia Cloud Technologies Users Group](https://www.meetup.com/philly-lync-users-group/) can be found in the repo as [DataFactoryAutomationPresentation.pdf](./DataFactoryAutomationPresentation.pdf)

- [Quickstart: Create a data factory and pipeline using .NET SDK](https://learn.microsoft.com/en-us/azure/data-factory/quickstart-create-data-factory-dot-net)
- [Azure SDK for .NET](https://github.com/Azure/azure-sdk-for-net)
- [Azure Portal](https://portal.azure.com/#home)
