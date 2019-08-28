var CareerCircleAPI = (function (apiUrl) {
    var _session_key = "ccapi";
    var _api_url = apiUrl.trim().replace(/\/$/, "");

    // setup axios instance which we will use for http requests to API
    var _http = axios.create({
        baseURL: apiUrl,

        transformRequest: [function (data, headers) {
            if (!SessionStorage.getJSON(_session_key))
                return data;

            headers['Authorization'] = 'Bearer ' + SessionStorage.getJSON(_session_key).accessToken;
            return data;
        }],
        headers: {
            'Content-Type': 'application/json'
        }
    });

    // on requests check token and set it
    _http.interceptors.request.use(
        function (config) {
            return new Promise(function (resolve, reject) {
                getToken().then(function (jwt) {
                        resolve(config);
                    })
                    .catch(function (err) {
                        // if I fail to get token then I must not be logged in
                        window.location = '/Session/SignIn' + '?redirectUri=' + encodeURIComponent(window.location);
                        reject(err);
                    });
            })
        }
    );

    // checks token to see if it is expired, if expired then get another one
    var getToken = function () {
        var jwt = SessionStorage.getJSON(_session_key);

        if (jwt == null || new Date() >= new Date(jwt.expiresOn)) {
            return new Promise(function (resolve, reject) {
                retrieveToken()
                    .then(function (response) {
                        SessionStorage.set(_session_key, JSON.stringify(response.data));
                        resolve(SessionStorage.getJSON(_session_key));
                    })
                    .catch(function (res) {
                        reject(res);
                    });
            });
        };

        return new Promise(function (resolve) {
            resolve(jwt);
        });
    };

    var signOut = function () {
        SessionStorage.clear();
    };

    // call to webapp to get token
    var retrieveToken = function () {
        return axios.get('/session/token');
    };

    // call to profile endpoint
    var getProfile = function () {
        return _http.get('/profile');
    };

    var deleteFile = function (fileId) {
        return new Promise(function (resolve) {
            getToken().then(function (jwt) {
                var path = '/subscriber/' + jwt.uniqueId + '/file/' + fileId;
                resolve(_http.delete(path));
            });
        });
    }


    var uploadAvatar = function (avatar) {
        var formData = new FormData();
        formData.append("avatar", avatar);
        return _http.post('/subscriber/avatar', formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
    }

    var removeAvatar = function () {
        return _http.delete('/subscriber/avatar');
    }

    var uploadResume = function (resume, parseResume) {
        var formData = new FormData();
        formData.append("resume", resume);

        if (parseResume == null)
            parseResume = false;

        formData.append("parseResume", parseResume);

        return _http.post('/resume/upload', formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
    }

    var scanResumeOnFile = function () {
        return _http.post('/resume/scan');
    };

    var getOffer = function (offerGuid) {
        return _http.get('/offers/' + offerGuid);
    }

    var claimOffer = function (offerGuid) {
        return _http.post('/offers/' + offerGuid + "/claim");
    };

    var getContacts = async function (page, pageSize, sorted, filtered, startDate, endDate, partner) {
        var pageIndex = page <= 0 ? 1 : page;
        var params = "";
        if (sorted && sorted.length > 0) {
            var params = "&sort=";
            for (var i = 0; i < sorted.length; i++) {
                var col = sorted[i];
                var sortType = col.desc ? "desc" : "asc";
                params += encodeURI(col.id + " " + sortType)
            }
        }
        if (filtered && filtered.length > 0) {
            for (var i = 0; i < filtered.length; i++) {
                var col = filtered[i];
                params += "&" + encodeURI(col.id) + "=" + encodeURI(col.value);
            }
        }

        if (startDate)
            params += "&startDate=" + moment(startDate).startOf('day').valueOf();

        if (endDate)
            params += "&endDate=" + moment(endDate).endOf('day').valueOf();

        if (partner)
            params += "&partnerId=" + partner;

        return await _http.get('/contact?page=' + pageIndex + '&pageSize=' + pageSize + params);
    }

    var getPartners = async function () {
        return await _http.get('/partners');
    }

    var getSubscriberActionsReport = async function (query) {
        return await _http.get(`/report/subscriber-actions${buildQuery(query)}`);
    }
    var getNotificationCountReport = async function (query) {
        return await _http.get(`/report/notification-reads${buildQuery(query)}`);
    }

    var getJobAppReport = async function (query) {
        return await _http.get(`/report/job-applications${buildQuery(query)}`);
    }

    var getJobAbandonmentCount = function (startDate, endDate) {
        return _http.get('/report/job-abandonment-count/?startDate=' + startDate + '&endDate=' + endDate);
    }

    var getTotalUserBreakdownByPartner = function (startDate, endDate) {
        return _http.get('/report/partners/?startDate=' + startDate + '&endDate=' + endDate);
    }

    var requestVerification = function (verifyUrl) {
        return _http.post('/subscriber/request-verification', JSON.stringify({
            verifyUrl: verifyUrl
        }));
    }

    var addJobFavorite = async function (jobGuid) {
        var subscriberGuid = SessionStorage.getJSON(_session_key) ? SessionStorage.getJSON(_session_key).uniqueId : null;
        return await _http.post('/job/favorite',
            JSON.stringify({
                jobPosting: {
                    jobPostingGuid: jobGuid
                },
                subscriber: {
                    subscriberGuid: subscriberGuid
                }
            }));
    }

    var deleteJobFavorite = async function (jobGuid) {
        return await _http.delete(`/job/favorite/${jobGuid}`);
    }


    var getResumeParseMergeQuestionnaire = async function (guid) {
        return await _http.get('/resume/profile-merge-questionnaire/' + guid);
    }

    var addJobAlert = async function (jobQuery, description, frequency, executionHour, executionMinute, executionDayOfWeek, timeZoneOffset, localDate) {
        var subscriberGuid = SessionStorage.getJSON(_session_key) ? SessionStorage.getJSON(_session_key).uniqueId : null;
        var jobPostingAlertDto = JSON.stringify({
            jobQuery: jobQuery,
            description: description,
            frequency: frequency,
            executionHour: parseInt(executionHour, 10),
            executionMinute: parseInt(executionMinute, 10),
            executionDayOfWeek: parseInt(executionDayOfWeek, 10),
            timeZoneOffset: parseInt(timeZoneOffset),
            localDate: localDate,
            subscriber: {
                subscriberGuid: subscriberGuid
            }
        });
        return await _http.post('/job/alert', jobPostingAlertDto);
    }

    var deleteJobAlert = async function (jobPostingAlertGuid) {
        return await _http.delete(`/job/alert/${jobPostingAlertGuid}`);
    }

    var getCompanies = async function () {
        return await _http.get('/companies');
    }

    var addCompany = async function (companyObj) {
        return await _http.post('/company/add', JSON.stringify(companyObj));
    }

    var editCompany = async function (companyObj) {
        return await _http.post('/company/update', JSON.stringify(companyObj));
    }

    var deleteCompany = async function (companyGuid) {
        return await _http.delete('/company/delete/' + companyGuid);
    }

    var getRecruiters = async function () {
        return await _http.get('/recruiters');
    }

    var getRecruiterDetails = async function (query) {
        return await _http.get(`/subscriber/subscriber-details${buildQuery(query)}`);
    }

    var addRecruiter = async function (recruiterObj) {
        return await _http.post('/recruiter/add', JSON.stringify(recruiterObj));
    }

    var editRecruiter = async function (recruiterObj) {
        return await _http.post('/recruiter/update', JSON.stringify(recruiterObj));
    }

    var deleteRecruiter = async function (recruiterObj) {
        return await _http.post('/recruiter/delete/', JSON.stringify(recruiterObj));
    }

    var getCourseSites = async function () {
        return await _http.get('/course-sites');
    }
     
    return {
        getProfile: getProfile,
        uploadResume: uploadResume,
        scanResumeOnFile: scanResumeOnFile,
        deleteFile: deleteFile,
        signOut: signOut,
        getContacts: getContacts,
        getPartners: getPartners,
        getSubscriberActionsReport: getSubscriberActionsReport,
        getJobAppReport: getJobAppReport,
        requestVerification: requestVerification,
        getOffer: getOffer,
        claimOffer: claimOffer,
        addJobFavorite: addJobFavorite,
        deleteJobFavorite: deleteJobFavorite,
        uploadAvatar: uploadAvatar,
        removeAvatar: removeAvatar,
        getResumeParseMergeQuestionnaire: getResumeParseMergeQuestionnaire,
        addJobAlert: addJobAlert,
        deleteJobAlert: deleteJobAlert,
        getCompanies: getCompanies,
        addCompany: addCompany,
        editCompany: editCompany,
        deleteCompany: deleteCompany,
        getRecruiters: getRecruiters,
        getRecruiterDetails: getRecruiterDetails,
        addRecruiter: addRecruiter,
        editRecruiter: editRecruiter,
        deleteRecruiter: deleteRecruiter,
        getNotificationCountReport: getNotificationCountReport,
        getCourseSites: getCourseSites,
        getJobAbandonmentCount: getJobAbandonmentCount,
        getTotalUserBreakdownByPartner: getTotalUserBreakdownByPartner,
    };

})(API_URL);