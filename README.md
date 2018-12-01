	
# Introduction

This repository represents a snapshot of my technical skills in December 2017. My coding practices and skills set have evolved since then. I was the only developer so was responsible for all operational and support tasks as well.

Technologies used: C#, ASP.NET MVC and Web API, Entity Framework 6, LINQ, Razor, HTML5, CSS, Bootstrap, JavaScript, JQuery, AJAX, Google Charts API, IIS, SQL Server


# Structure of the solution
  -	MainIntranetApp : main service accessible through AD authentication and a browser for end users (colleagues)
  -	CompanyDbUpdate: runs on the server daily to update the company database
  
	    Use the two following web APIs for the update: CompanyDbWebAPI, ThirdPartyDbWebAPI

  -	CompanyDbWebAPI: web API hosted on the server to CRUD the company data base
  
	     This project also defines the structure of the company data base via entity framework code first
  -	ThirdPartyDbWebAPI: web API hosted on the server to extract information from the third party booking system’s database


# Origin
The project was a response to a few reporting issues:
  - the KPIs reports were created with Excel, automated and connected to the third party booking system's database:
      - It was getting harder to modify them on an individual basis, their complexity was increased to compensate for the limitation of Excel
      - Remotely working colleagues would only be able to access them from a terminal server connection, adding strain to it
      - In some cases I had to create desktop applications to communicate with the CRM's web api and generate the Excel file, they had to be installed on each machine with administrator credentials, same issue when updating the applications
          
The solution was to use the browser to view data and charts. It became an intranet web application providing other services to colleagues.       



# MainIntranetApp data sources
The first data source is from the third party booking system’s database. I use Entity Framework from an existing database which generate the .edmx model and the necessary entity classes. These and the db context will not be shown in this repository, only their uses in this project will be shown. 

To avoid conflict with the previous Entity framework model, I used the second data source which is the solution's database through a Web API defined on a separate project: CompanyDbWebAPI.

  
