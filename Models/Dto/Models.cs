using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEbAPi.Models
{

    // Добавление в корзину
    public class AddToCartRequest
    {
        public int CatalogId { get; set; }
    }

    // Товар в корзине
    public class CartItemDto
    {
        public int PosOrderId { get; set; }
        public int CatalogId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class CartResponse
    {
        public IEnumerable<PosOrder> Items { get; set; } = new List<PosOrder>();
        public decimal TotalSum { get; set; }
    }



    // Карточка товара
    public class CatalogDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public List<ReviewDto> Reviews { get; set; } = new();
    }

    // Отзыв
    public class ReviewDto
    {
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Models/LoginModel.cs
    
        public class LoginModel
        {
            [Required(ErrorMessage = "Email обязателен")]
            [EmailAddress(ErrorMessage = "Некорректный email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Пароль обязателен")]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }
    


public class RegisterModel
{
    [Required(ErrorMessage = "Логин обязателен")]
    public string Login { get; set; }

    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Пароль обязателен")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; }
}

    // Models/LoginRequest.cs
    public class LoginRequest
    {
        [Required(ErrorMessage = "Логин обязателен")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "Логин обязателен")]
        public string Login { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; }
    }

    public class AverageRatingResponse
    {
        public double AverageRating { get; set; }
    }

    public class ProductDetailsResponse
    {
        public Catalog Product { get; set; }
        public IEnumerable<Review> Reviews { get; set; }
    }

    public class ReviewRequest
    {
        public int ProductId { get; set; }
        public string Text { get; set; }
        public int Rating { get; set; }


        // Models/RegisterRequest.cs

    }


    public static class UserRoles
    {
        public const int Admin = 1;
        public const int User = 2;

        public const string AdminName = "admin";
        public const string UserName = "user";
    }
}
