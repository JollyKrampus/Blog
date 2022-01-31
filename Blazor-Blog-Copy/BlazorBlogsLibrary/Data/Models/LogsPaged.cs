using System.Collections.Generic;
using BlazorBlogs.Data.Models;

namespace BlazorBlogsLibrary.Data.Models
{
    public class LogsPaged
    {
        public List<Logs> Logs { get; set; }
        public int LogCount { get; set; }
    }
}