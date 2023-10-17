# Trade Market - DAL


## Domain description

Supermarkets sell goods of various categories. The customers can shop anonymously or by logging in. When buying, a receipt is created with a list of goods purchased in a particular market.


## Task

Develop a Data Access Layer (DAL) for an electronic system **"Trade Market"** with Three-Layer Architecture in dynamic library form named “Data”.
![Data Entities](/Data/DataEntities_Scheme.jpeg)

The structure of the DAL project in the final form:
- The folder **Entities** contains classes of entities – make entities according to the diagram (Fig.). Every entity should inherit **BaseEntity { int Id }**.
- The folder **Interfaces** contains repository interfaces of entities and the interface of their merge point.
- The folder **Repositories** contains repository classes that implement interfaces from the folder **Interfaces** – implement all the repositories according to the interfaces from the Interfaces folder.
- The root folder of the project contains **MarketDBContext.cs** file for project entity context - implement the class **MarketDBContext**.
- The root folder of the project contains **UnitOfWork.cs** file. This class is entry point for all repositories to get access to DAL from the business logic.  Implement the class **UnitOfWork** based on the **IUnitOfWork** interface.
- The folder **Migrations** contains project database migration files – use migrations by developing DB.  
Instructions on **how to create migrations** can be found in file **_Data / Migrations_Overview.md_.**
