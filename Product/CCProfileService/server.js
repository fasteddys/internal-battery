

require('dotenv').config()
const express = require('express');
const talentAPI = require('@google-cloud/talent');
 
'use strict';
var http = require('http');
var port = process.env.PORT;

const app = express() 
app.get('/', (req, res) => res.send('Hello World!'))
// app.listen(port, () => console.log(`Example app listening on port ${port}!`))
app.listen(port)

app.get('/', function (req, res) {
    res.send('hello world')
})


app.get('/jimbo', function (req, res) {
    res.send('hello jimbo')
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
 
