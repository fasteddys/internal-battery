using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace UpDiddy.ViewModels.ButterCMS
{ 
    public class BlogPageViewModel
    {
      public IEnumerable<BlogPostViewModel> BlogPosts { get; set; }
    }    
}
