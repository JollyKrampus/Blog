using System.Collections.Generic;
using BlazorBlogs.Data.Models;

namespace BlazorBlogsLibrary.Data.Models
{
    public class BlogsPaged
    {
        public List<Blogs> Blogs { get; set; }
        public int BlogCount { get; set; }
    }
}