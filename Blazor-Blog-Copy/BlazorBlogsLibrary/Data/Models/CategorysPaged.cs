using System.Collections.Generic;

namespace BlazorBlogsLibrary.Data.Models
{
    public class CategorysPaged
    {
        public List<CategoryDTO> Categorys { get; set; }
        public int CategoryCount { get; set; }
    }
}