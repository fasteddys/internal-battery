

require('dotenv').config()
const express = require('express');
const talentAPI = require('@google-cloud/talent');
 
'use strict';
var http = require('http');
var port = process.env.PORT;

const app = express() 


/**
 * Create a Tenant.
 */
const createTenant = async (
    tenantServiceClient,
    parent,
    tenantToBeCreated
) => {
    try {

        const request = {
            parent: parent,
            tenant: tenantToBeCreated,
        };

        const tenantCreated = await tenantServiceClient.createTenant(request)
            .then((x) => {

                console.log(`Tenant created: ${JSON.stringify(tenantCreated[0])}`);
            });

      
        return tenantCreated[0];
    } catch (e) {
        console.error(`Got exception while creating tenant!`);
        throw e;
    }
};


function test() {

    throw ("test exception ");

}



app.get('/', (req, res) => res.send('Hello World!'))
// app.listen(port, () => console.log(`Example app listening on port ${port}!`))
app.listen(port)

app.get('/', function (req, res) {
    res.send('hello world');
})


app.get('/:name', function (req, res) {
    res.send( 'hello ' + req.params.name )
})


app.get('/create-tenant/:tenantName', function (req, res) {

    let tenantServiceClient = new talentAPI.TenantServiceClient({
        projectId: process.env.ProjectId,
        keyFilename: process.env.KeyFilePath,
    });


    try {   
        test();
        createTenant(talentAPI.tenantServiceClient, process.env.ProjectName, 'CareerCircleProfiles-Dev')
        res.send('Tenant ' + req.params.tenantName + ' has been created for ' + process.env.projectId);
    }
    catch (ex) {
        res.send('Error creating tenant:' + ex);
    }

  
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
 
