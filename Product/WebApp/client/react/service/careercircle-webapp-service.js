

export default class CareerCircleWebAppService  {
    

    OpenBlogPage(slug){
        var formData = new FormData();
        formData.append("slug", slug);
        return _http.post('/Blog/', JSON.stringify(slug), {
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function (response) {
           console.log(response);
        })
        .catch(function (error) {
            console.log(error);
        });

    }
}