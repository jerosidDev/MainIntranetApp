# MainIntranetApp
Intranet web application for reporting, utilities, procedures, KPIs visuals, calendars, links.

# Origin
The project was a response to a few reporting issues:
  - the KPI reports were created with Excel, automated and connected to the third party booking system's database:
      - it was getting harder to modify them on an individual basis, their complexity was increased to compensate for the limitation of Excel
      - remotely working colleagues would only be able to access them from a terminal server connection, adding strain to it
      - in some cases I had to create desktop applications to communicate with the CRM's web api and generate the Excel file, they had to be installed on each machine with administrator credentials, same issue when updating the applications
          
The solution was to use the browser to view data and charts. It became an intranet web application providing services to colleagues.       

# Structure of the solution
  -	MainIntranetApp : main service accessible through AD identification and a browser for end users (colleagues)
  -	CompanyDbUpdate: run on the server daily to update the company database
      o	Use the two following internal web APIs for the update
  -	CompanyDbWebAPI: internal web API hosted on the server to CRUD the company data base
      o	Also defines the structure of the company data base via entity framework code first
  -	ThirdPartyDbWebAPI: internal web API hosted on the server to extract information from the third party booking system’s database
  
# Objective
It is to demonstrate my practical knowledge of the technologies I use on a daily basis. The repositories will contain some of the code contained in the differents projects I work on.  
I would appreciate feedbacks from experienced developers regarding the code, the architecture or different design patterns. When I started the solution, I was on a "make the solution works" mode focused on achieving succesful proofs of concept. I am now on a "increase the solution's quality and maintainability" mode. 
