class BasicResponse {
    constructor(code, description,data) {
        this.StatusCode = code;
        this.Description = description;
        this.Data = data;
    }
}
module.exports = BasicResponse;