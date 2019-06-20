module.exports = async function (creds, tenantName) {


    try {

    const profiles = await require('./agProfiles')(creds);

    module.parseResumeToProfile = function(base64Doc) {
        return profiles.parseResumeToProfile(base64Doc);
    }

    module.createTenant = function (tenantName) {
        return profiles.createTenant(tenantName);
    }

    module.listTenant = function (tenantName) {
        return profiles.listTenant(tenantName);
    }

    if(tenantName) {
      const tenant = await profiles.getTenant(tenantName);

    console.log(`Tenant: ${tenant.name} is initialized`);

    module.getProfile = function (profileName) {
        return profiles.getProfile(profileName);
    }

    module.putProfile = function (profile) {
        return profiles.putProfile(tenant.name,profile);
    }

    module.basicQuery = function (query) {
        return profiles.basicQuery(tenant.name,query);
    }

    module.query = function (requestBody) {
        return profiles.query(tenant.name,requestBody);
    }

    module.listProfiles = function (pageSize) {
        return profiles.listProfile(tenant.name, pageSize);
    }

    
}
    } catch (err) {
        console.log(`error: ${err}`);
    }
    return module;
}