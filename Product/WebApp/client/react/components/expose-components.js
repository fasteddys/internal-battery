import React from 'react';
import ReactDOM from 'react-dom';
import ReactDOMServer from 'react-dom/server';

import ResumeUpload from './ResumeUpload';
import PATable from './reports/PATable';

global.React = React;
global.ReactDOM = ReactDOM;
global.ReactDOMServer = ReactDOMServer;

// components
global.ResumeUpload = ResumeUpload;

// reports
global.PATable = PATable;

// work around for require (until gulp refactor)
import buildQuery from 'odata-query';
global.buildQuery = buildQuery;