
'use strict';

const dotenv = require('dotenv').config()
const express = require('express');
const talentAPI = require('@google-cloud/talent');
const basicResponse = require('basicresponse');
const http = require('http');

const port = process.env.PORT;
const app = express() 

const profileSearchOption = { autoPaginate: false };



// Parse URL-encoded bodies (as sent by HTML forms)
app.use(express.urlencoded());
// Parse JSON bodies (as sent by API clients)
app.use(express.json());
app.listen(port)


// default 
app.get('/', function (req, res) { 
    res.send('CareerCircle Profile API (version:' + process.env.version + ')');
})

 


 
app.post('/tenant/:tenantName', async (req, res) => {

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
        let r = new basicResponse(200, "ok", tenantCreated[0]);
        res.send(r);
    }
    catch (e)
    {
        let r = new basicResponse(400, e.details);
        res.send(r);
    }
  
})


app.get('/tenant', async (req, res) => {

    let tenantServiceClient = new talentAPI.TenantServiceClient({
        projectId: process.env.ProjectId,
        keyFilename: process.env.KeyFilePath,
    });

    var tenants = [];
    try {

        await tenantServiceClient.listTenants({ parent: process.env.ProjectId })
              .then(responses  =>
              {
                 const resources = responses[0];
                  for (const resource of resources) {
                      tenants.push(resource);
            
                 }
              })
              .catch(err =>
              {
                 console.error(err);
              });
        
        let r = new basicResponse(200, "ok",tenants);
        res.send(r);
    }
    catch (e) {
        let r = new basicResponse(400, e.details);
        res.send(r);
    }

})



 







app.post('/profile', async (req, res) => {

    try { 
        let profileServiceClient = new talentAPI.ProfileServiceClient({
            projectId: process.env.ProjectId,
            keyFilename: process.env.KeyFilePath,
        });
 
        let profileToBeCreated = req.body; 
        let request = {
            parent: process.env.tenantName,
            profile: profileToBeCreated,
        };

        let profileCreated = await profileServiceClient.createProfile(request);
        let r = new basicResponse(200, "ok", profileCreated[0]);        
        res.send(r);
    } catch (e)
    {
        let r = new basicResponse(400, e.details);
        res.send(r);
    }
})


app.put('/profile', async (req, res) => {

    try
    {
        let profileServiceClient = new talentAPI.ProfileServiceClient({
            projectId: process.env.ProjectId,
            keyFilename: process.env.KeyFilePath,
        });
 
        let profileToBeUpdated = req.body;
 
        let request = {
            parent: process.env.tenantName,
            profile: profileToBeUpdated,
        };

        let profileUpdated = await profileServiceClient.updateProfile(request);
        let r = new basicResponse(200, "ok", profileUpdated[0]);        
        res.send(r);
    } catch (e)
    {     
        let r = new basicResponse(400, e.details);
        res.send(r);
    }
})


app.delete('/profile/:profileName', async (req, res) => {

    try {
        let profileServiceClient = new talentAPI.ProfileServiceClient({
            projectId: process.env.ProjectId,
            keyFilename: process.env.KeyFilePath,
        });

        let request = {
            name: req.params.profileName
        };

        await profileServiceClient.deleteProfile(request);
        let r = new basicResponse(200, "ok" );
        res.send(r);
    }
    catch (e)
    {
        let r = new basicResponse(400, e.details);
        res.send(r);
    }
})


app.get('/profile/:profileName', async (req, res) =>  {

    try {    
 
        let request = {
            name: req.params.profileName
        };

        let profileServiceClient = new talentAPI.ProfileServiceClient({
            projectId: process.env.ProjectId,
            keyFilename: process.env.KeyFilePath,
        });

        let profileExisted = await profileServiceClient.getProfile(request);

        let r = new basicResponse(200, "ok", profileExisted[0]);   
        res.send(r);
    } catch (e)
    {
        let r = new basicResponse(400, e.details);
        res.send(r);               
    } 
})


app.get('/profile-search', async (req, res) => {

   

    try {

        let profileServiceClient = new talentAPI.ProfileServiceClient({
            projectId: process.env.ProjectId,
            keyFilename: process.env.KeyFilePath,
        });

        /*
        let request = {
            parent: process.env.tenantName,
            profileQuery: {

                "query": null,
                "locationFilters": [
                    {
                        "address": "21204",
                        "regionCode": "us",
                        "latLng": null,
                        "distanceInMiles": 0,
                        "telecommutePreference": 0,
                        "negated": false
                    }
                ],
                "jobTitleFilters": null,
                "employerFilters": null,
                "educationFilters": null,
                "skillFilters": null,
                "workExperienceFilters": null,
                "timeFilters": null,
                "hirableFilter": null,
                "applicationDateFilters": null,
                "applicationOutcomeNotesFilters": null,
                "applicationJobFilters": null,
                "customAttributeFilter": null,
                "candidateAvailabilityFilters": null

            }
            // requestMetadata: { allowMissingIds : true}
        }

        let test = JSON.stringify(req.body);
        */


       // requestMetadata: { domain: "www.careercircle.com", sessionId : "not applicable", userId : "jim", allowMissingIds : true }

        let searchResults = await profileServiceClient.searchProfiles(req.body, profileSearchOption);
     //   let searchResults = await profileServiceClient.searchProfiles(request, profileSearchOption);
        let r = new basicResponse(200, "ok", searchResults[2]);
        res.send(r);
    }
    catch (e)
    {
        let r = new basicResponse(400, e.details);
        res.send(r);
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
 
