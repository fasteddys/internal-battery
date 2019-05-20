import React from 'react';
import ReactDOM from 'react-dom';
import ReactDOMServer from 'react-dom/server';

import ResumeUpload from './ResumeUpload';
import SATable from './reports/SATable';
import JobAppReportTable from './reports/JobAppReportTable';
import BrowseJobsByCategory from './jobs/BrowseCategory';
import BrowseJobsByLocation from './jobs/BrowseLocation';
import BrowseJobsByIndustry from './jobs/BrowseIndustry';

global.React = React;
global.ReactDOM = ReactDOM;
global.ReactDOMServer = ReactDOMServer;

// components
global.ResumeUpload = ResumeUpload;
global.BrowseJobsByCategory = BrowseJobsByCategory;
global.BrowseJobsByLocation = BrowseJobsByLocation;
global.BrowseJobsByIndustry = BrowseJobsByIndustry;

// reports
global.SATable = SATable;
global.JobAppReportTable = JobAppReportTable;

// work around for require (until gulp refactor)
import buildQuery from 'odata-query';
global.buildQuery = buildQuery;