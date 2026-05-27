using DataAccessLayer.Models;
using System.Collections.Generic;

namespace DataAccessLayer.Repositories.Interfaces;
public interface ICategoryRepository
{
    List<Category> GetAllCategories();
    Category? GetCategoryById(int id);
    void AddCategory(Category category);
    void UpdateCategory(Category category);
}