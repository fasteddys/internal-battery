

require('dotenv').config()
const express = require('express');
const talentAPI = require('@google-cloud/talent');
 
'use strict';
var http = require('http');
var port = process.env.PORT;

const app = express() 

// Parse URL-encoded bodies (as sent by HTML forms)
app.use(express.urlencoded());

// Parse JSON bodies (as sent by API clients)
app.use(express.json());

//app.get('/', (req, res) => res.send('Hello World!'))
// app.listen(port, () => console.log(`Example app listening on port ${port}!`))
app.listen(port)

app.get('/', function (req, res) { 
    res.send('hello world');
})

 


//  Existing Test tenants:
 
//  Name: projects/jobboardpilot/tenants/d927831d-16f8-492b-bb04-700d949b0633 External Id : tenant2
//  Name: projects/jobboardpilot/tenants/616966ef-f93e-47dd-81dd-63b01c47f195 External Id : tenant3
//  Name: projects/jobboardpilot/tenants/3d27fc68-8151-40de-96f7-cb11d8b7b252  External Id: cc-profiles-dev
app.get('/create-tenant/:tenantName', async (req, res) => {

    let tenantServiceClient = new talentAPI.TenantServiceClient({
        projectId: process.env.ProjectId,
        keyFilename: process.env.KeyFilePath,
    });
 
    try {  
        
        let  tenantToCreate = {
            externalId: req.params.tenantName,
            usageType: 'ISOLATED'
      
        }

        let  request = {
            parent: process.env.ProjectId,
            tenant: tenantToCreate,
        };

        let tenantCreated = await tenantServiceClient.createTenant(request);        

        res.send(JSON.stringify(tenantCreated[0])) ;
    }
    catch (ex) {
        res.send('Error creating tenant:' + ex.message);
    }
  
})


app.post('/create-profile', async (req, res) => {

    try {

        console.log('create-profile 1');
        let profileServiceClient = new talentAPI.ProfileServiceClient({
            projectId: process.env.ProjectId,
            keyFilename: process.env.KeyFilePath,
        });
        
     //   let profileToBeCreated = { 
      //          "externalId": "TestId"
      //  }

        let profileToBeCreated = req.body;
        
        let request = {
            parent: "projects/jobboardpilot/tenants/3d27fc68-8151-40de-96f7-cb11d8b7b252",
            profile: profileToBeCreated,
        };

        let profileCreated = await profileServiceClient.createProfile(request);

        res.send(JSON.stringify(profileCreated[0]));
    } catch (e) {
        console.error(`Got exception while creating profile!`);
        res.send('Got exception while creating profile! ');
    }

})


app.post('/testpost', function (req, res) {
     res.send(req.body);    
})





















/*
var app = express();
app.use(bodyParser.urlencoded({ extended: true }))
app.use(bodyParser.json())

const port = process.env.SomePort || process.env.SomePORT_Other;
app.listen(port);
*/

/* 
http.createServer(function (req, res) {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Hello World\n' + process.env.DB_HOST);
}).listen(port);
*/
 
