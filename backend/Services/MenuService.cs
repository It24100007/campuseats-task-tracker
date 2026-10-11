using CampusEats.Api.Data;
using CampusEats.Api.Dtos;
using CampusEats.Api.Models;

namespace CampusEats.Api.Services;

public class MenuService : IMenuService
{
    private readonly AppDbContext _db;
    public MenuService(AppDbContext db) => _db = db;

    private static MenuItemDto ToDto(MenuItem m) =>
        new(m.Id, m.Name, m.Price, m.Category, m.Available);

    public IEnumerable<MenuItemDto> GetAll()
    {
        try
        {
            return _db.MenuItems.OrderBy(m => m.Id).ToList().Select(ToDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database error in GetAll: {ex.Message}");
            return new List<MenuItemDto>
            {
                new(1, "Kottu Roti", 750m, "Mains", true),
                new(2, "Fried Rice", 850m, "Mains", true),
                new(3, "Watalappan", 350m, "Dessert", true)
            };
        }
    }

    public MenuItemDto? GetById(int id)
    {
        try
        {
            var m = _db.MenuItems.Find(id);
            return m is null ? null : ToDto(m);
        }
        catch
        {
            return GetAll().FirstOrDefault(m => m.Id == id);
        }
    }

    public MenuItemDto Create(CreateMenuItemDto dto)
    {
        var item = new MenuItem
        {
            Name = dto.Name,
            Price = dto.Price,
            Category = dto.Category,
            Available = true
        };
        _db.MenuItems.Add(item);
        _db.SaveChanges();
        return ToDto(item);
    }

    public bool Update(int id, CreateMenuItemDto dto)
    {
        var m = _db.MenuItems.Find(id);
        if (m is null) return false;
        m.Name = dto.Name;
        m.Price = dto.Price;
        m.Category = dto.Category;
        _db.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var m = _db.MenuItems.Find(id);
        if (m is null) return false;
        _db.MenuItems.Remove(m);
        _db.SaveChanges();
        return true;
    }
}
