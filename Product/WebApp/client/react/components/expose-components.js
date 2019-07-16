import React from 'react';
import ReactDOM from 'react-dom';
import ReactDOMServer from 'react-dom/server';
import ResumeMerge from './ResumeMerge';
import ResumeUpload from './ResumeUpload';
import Checkbox from './Checkbox';
import Favorite from './jobs/Favorite';
import SATable from './reports/SATable';
import JobAppReportTable from './reports/JobAppReportTable';
import BrowseJobs from './jobs/BrowseJobs';
import JobPostingAlert from './jobs/JobPostingAlert';
import Select from './Select';
import Validate from './Validate';
import TextArea from './TextArea';
import Breadcrumb from './breadcrumbs/Breadcrumb';

global.React = React;
global.ReactDOM = ReactDOM;
global.ReactDOMServer = ReactDOMServer;

// components
global.ResumeUpload = ResumeUpload;
global.ResumeMerge = ResumeMerge;
global.Checkbox = Checkbox;
global.Favorite = Favorite;
global.BrowseJobs = BrowseJobs;
global.JobPostingAlert = JobPostingAlert;
global.Select = Select;
global.Validate = Validate;
global.TextArea = TextArea;
global.Breadcrumb = Breadcrumb;

// dashboard
import Dashboard from './dashboard/dashboard';
global.Dashboard = Dashboard;
import NotificationListing from './dashboard/dashboard-helpers';
global.NotificationListing = NotificationListing;
import NotificationView from './dashboard/dashboard-helpers';
global.NotificationView = NotificationView;
import NotificationItem from './dashboard/dashboard-helpers';
global.NotificationItem = NotificationItem;

// reports 
global.SATable = SATable;
global.JobAppReportTable = JobAppReportTable;

// work around for require (until gulp refactor)
import buildQuery from 'odata-query';
global.buildQuery = buildQuery;

//Companies
import CompaniesGrid from './company/CompaniesGrid';
global.CompaniesGrid = CompaniesGrid;

//Recruiters
import RecruitersGrid from './recruiters/RecruitersGrid';
global.RecruitersGrid = RecruitersGrid;