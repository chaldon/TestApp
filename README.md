# Skills Test Project

 REST API endpoints to support basic ecommerce system. Live demo (temporarily) available here http://20.36.190.80/swagger/index.html (Azure/Ubuntu VM + Azure SQL Server)

 Should be easy to run from Visual Studio 2019.
 The only thing which requires configuration is a section of appsettings.json/appsettings.Development.json file:

`   "ConnectionStrings": {
    "TestAppDatabase": "Data Source=(localdb)\\ProjectsV13;Initial Catalog=TestApp;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
  }`

The app store data in a single Data\TestApp.mdf file which contains seed data and slightly modified databse structure.
Sadly, it was created with SQL Server 2016 LocalDB and may not work on your machine if you have newer version of the LocalDB.
Probably you can to upgrade it to the latest version, see https://docs.microsoft.com/en-us/visualstudio/data-tools/upgrade-dot-mdf-files?view=vs-2019 

Another option is to use SQL server from the live demo. I''l send connection string seprately.

Project configured to open Swagger UI on start of the project at https://localhost:5001/swagger/index.html . I hope that API is simple and self-descriptive.

Possible improvements:
* Improve filtering/sorting (currently it have a limited set of filters and ordering is predefined to the primary key)
* Better test coverage (current project have a unit and integration tests for ProductController only)
* Authentication/Authorization
* Refactor db in a way which allow orders to have have multiple offers (it's 1:1 now)
* Use full-text search on a text fields
* Actions audit (like orders modifications)
* Implement HATEOAS - https://en.wikipedia.org/wiki/HATEOAS
* Switch to GraphQL :-)
