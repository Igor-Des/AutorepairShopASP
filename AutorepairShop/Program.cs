using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using AutorepairShop.Data;
using AutorepairShop.Models;
using AutorepairShop.Services;
using AutorepairShop.Infrastructure;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


var builder = WebApplication.CreateBuilder(args);

string connString = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Services.AddDbContext<AutorepairContext>(options => options.UseSqlServer(connString));
builder.Services.AddMemoryCache();

builder.Services.AddDistributedMemoryCache();// добавляем IDistributedMemoryCache
builder.Services.AddSession();  // добавляем сервисы сессии

builder.Services.AddScoped<ICachedOwnersService, CachedOwnersService>();
builder.Services.AddScoped<ICachedCarsService, CachedCarsService>();
builder.Services.AddScoped<ICachedPaymentsService, CachedPaymentsService>();


var app = builder.Build();

app.UseSession();  // добавляем middleware для работы с сессиями

app.Map("/info", (appBuilder) =>
{
    appBuilder.Run(async (context) =>
    {
        // Формирование строки для вывода 
        string strResponse = "<HTML><HEAD><TITLE>info</TITLE></HEAD>" +
        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
        "<BODY><H1>Информация:</H1>";
        strResponse += "<BR> Сервер: " + context.Request.Host;
        strResponse += "<BR> Путь: " + context.Request.PathBase;
        strResponse += "<BR> Протокол: " + context.Request.Protocol;
        strResponse += "<BR><A href='/'>Главная</A></BODY></HTML>";
        // Вывод данных
        await context.Response.WriteAsync(strResponse);
    });
});

app.Map("/owners", (appBuilder) =>
{
    appBuilder.Run(async (context) =>
    {
        ICachedOwnersService cachedOwner = context.RequestServices.GetService<ICachedOwnersService>();
        IEnumerable<Owner> owners = cachedOwner.GetOwners();
        string HtmlString = "<HTML><HEAD><TITLE>Owner</TITLE></HEAD>" +
        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
        "<BODY><H1>Список владельцев авто</H1>" +
        "<TABLE BORDER=1>";
        HtmlString += "<TR>";
        HtmlString += "<TH>Имя</TH>";
        HtmlString += "<TH>Фамилия</TH>";
        HtmlString += "<TH>Отчество</TH>";
        HtmlString += "<TH>Телефон</TH>";
        HtmlString += "<TH>Адрес</TH>";
        HtmlString += "<TH>Номер вод.уд.</TH>";
        HtmlString += "</TR>";
        foreach (Owner owner in owners)
        {
            HtmlString += "<TR>";
            HtmlString += "<TD>" + owner.FirstName + "</TD>";
            HtmlString += "<TD>" + owner.MiddleName + "</TD>";
            HtmlString += "<TD>" + owner.LastName + "</TD>";
            HtmlString += "<TD>" + owner.Phone + "</TD>";
            HtmlString += "<TD>" + owner.Address + "</TD>";
            HtmlString += "<TD>" + owner.DriverLicenseNumber + "</TD>";
            HtmlString += "</TR>";
        }
        HtmlString += "</TABLE>";
        HtmlString += "<BR><A href='/'>Главная</A></BR>";
        HtmlString += "</BODY></HTML>";
        await context.Response.WriteAsync(HtmlString);
    });
});


app.Map("/cars", (appBuilder) =>
{
    appBuilder.Run(async (context) =>
    {
        ICachedCarsService cachedCars = context.RequestServices.GetService<ICachedCarsService>();
        IEnumerable<Car> cars = cachedCars.GetCars(50);
        string HtmlString = "<HTML><HEAD><TITLE>Car</TITLE></HEAD>" +
        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
        "<BODY><H1>Список автомобилей</H1>" +
        "<TABLE BORDER=1>";
        HtmlString += "<TR>";
        HtmlString += "<TH>Марка</TH>";
        HtmlString += "<TH>Мощность</TH>";
        HtmlString += "<TH>Цвет</TH>";
        HtmlString += "<TH>Гос номер</TH>";
        HtmlString += "<TH>Код владелеца</TH>";
        HtmlString += "<TH>Год</TH>";
        HtmlString += "<TH>ВИН</TH>";
        HtmlString += "<TH>Номер двигателя</TH>";
        HtmlString += "<TH>Дата поступления</TH>";
        HtmlString += "</TR>";
        foreach (Car car in cars)
        {
            HtmlString += "<TR>";
            HtmlString += "<TD>" + car.Brand + "</TD>";
            HtmlString += "<TD>" + car.Power + "</TD>";
            HtmlString += "<TD>" + car.Color + "</TD>";
            HtmlString += "<TD>" + car.StateNumber + "</TD>";
            HtmlString += "<TD>" + car.OwnerId + "</TD>";
            HtmlString += "<TD>" + car.Year + "</TD>";
            HtmlString += "<TD>" + car.VIN + "</TD>";
            HtmlString += "<TD>" + car.EngineNumber + "</TD>";
            HtmlString += "<TD>" + car.AdmissionDate + "</TD>";
            HtmlString += "</TR>";
        }
        HtmlString += "</TABLE>";
        HtmlString += "<BR><A href='/'>Главная</A></BR>";
        HtmlString += "</BODY></HTML>";
        await context.Response.WriteAsync(HtmlString);
    });
});

// cookies
app.Map("/carsearch", (appBuider) =>
{
    appBuider.Run(async (context) =>
    {
        ICachedCarsService cachedCar = context.RequestServices.GetService<ICachedCarsService>();
        string brandStr = null;
        if (context.Request.Cookies.ContainsKey("brand")) { brandStr = context.Request.Cookies["brand"]; }
        IEnumerable<Car> cars = cachedCar.GetCars(brandStr, 50);

        string strResponse = "<HTML><HEAD><TITLE>Cars search</TITLE></HEAD>" +
        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
        "<BODY><FORM action ='/carsearch' / >" +
        "Имя:<BR><INPUT type = 'text' name = 'Brand' value = " + brandStr + ">" +
        "<BR><BR><INPUT type ='submit' value='save in cookies and show table'></FORM>" +
        "<TABLE BORDER = 1>";
        strResponse += "<TH>Марка</TH>";
        strResponse += "<TH>Мощность</TH>";
        strResponse += "<TH>Цвет</TH>";
        strResponse += "<TH>Гос номер</TH>";
        strResponse += "<TH>Код владелеца</TH>";
        strResponse += "<TH>Год</TH>";
        strResponse += "<TH>ВИН</TH>";
        strResponse += "<TH>Номер двигателя</TH>";
        strResponse += "<TH>Дата поступления</TH>";
        brandStr = context.Request.Query["Brand"];
        if (brandStr != null)
        {
            context.Response.Cookies.Append("Brand", brandStr);
        }
        foreach (Car car in cars.Where(i => i.Brand.Trim() == brandStr))
        {
            strResponse += "<TR>";
            strResponse += "<TD>" + car.Brand + "</TD>";
            strResponse += "<TD>" + car.Power + "</TD>";
            strResponse += "<TD>" + car.Color + "</TD>";
            strResponse += "<TD>" + car.StateNumber + "</TD>";
            strResponse += "<TD>" + car.OwnerId + "</TD>";
            strResponse += "<TD>" + car.Year + "</TD>";
            strResponse += "<TD>" + car.VIN + "</TD>";
            strResponse += "<TD>" + car.EngineNumber + "</TD>";
            strResponse += "<TD>" + car.AdmissionDate + "</TD>";
            strResponse += "</TR>";
        }
        strResponse += "</TABLE>";
        strResponse += "<BR><A href='/'>Главная</A></BR>";
        strResponse += "</BODY></HTML>";
        await context.Response.WriteAsync(strResponse);
    });
});


// sessions
app.Map("/ownersearch", (appBuilder) =>
{
    appBuilder.Run(async (context) =>
    {
        ICachedOwnersService cachedOwner = context.RequestServices.GetService<ICachedOwnersService>();
        string firstName = "m";
        if (context.Session.Keys.Contains("firstName"))
        {
            firstName = context.Session.GetString("firstName");
        }
        IEnumerable<Owner> owners = cachedOwner.GetOwners();
       
        if (Convert.ToString(context.Request.Query["firstName"]) != null)
        {

            firstName = Convert.ToString(context.Request.Query["firstName"]);
            context.Session.SetString("firstName", firstName);
        }

        string strResponse = "<HTML>" +
        "<HEAD>" +
            "<TITLE>Owner search firstName</TITLE>" +
        "</HEAD>" +
        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
        "<BODY>" +
        "<FORM action ='/ownersearch' / >" +
            "FirstName :<BR><INPUT type = 'text' name = 'firstName' value = " + firstName + ">" +
            "<INPUT type ='submit' value='save in session and show table'>" +
        "</FORM>" +
        "<TABLE BORDER = 1>";
        strResponse += "<TR>";
        strResponse += "<TH>Имя владельца</TH>";
        strResponse += "<TH>Фамилия владельца</TH>";
        strResponse += "<TH>Отчество владельца</TH>";
        strResponse += "<TH>Телефон владельца</TH>";
        strResponse += "<TH>Адрес владельца</TH>";
        strResponse += "<TH>Номер вод.уд.</TH>";
        strResponse += "</TR>";
        foreach (Owner owner in owners.Where(i => i.FirstName.Trim() == firstName))
        {
            strResponse += "<TR>";
            strResponse += "<TD>" + owner.FirstName + "</TD>";
            strResponse += "<TD>" + owner.MiddleName + "</TD>";
            strResponse += "<TD>" + owner.LastName + "</TD>";
            strResponse += "<TD>" + owner.Phone + "</TD>";
            strResponse += "<TD>" + owner.Address + "</TD>";
            strResponse += "<TD>" + owner.DriverLicenseNumber + "</TD>";
            strResponse += "</TR>";
        }
        strResponse += "</TABLE>";
        strResponse += "<BR><A href='/'>Главная</A></BR>";
        strResponse += "</BODY></HTML>";
        await context.Response.WriteAsync(strResponse);
    });
});


app.MapGet("/", (context) =>
{
    ICachedCarsService cachedCar = context.RequestServices.GetService<ICachedCarsService>();
    cachedCar?.AddCars("Rover"); // как только запускается сервак в куки закидывается бренд авто

    string HtmlString = "<HTML><HEAD><TITLE>autorepair shop</TITLE></HEAD>" +
                "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                "<BODY><H1>Главная</H1>";
    HtmlString += "<H2>autorepair shop</H2>";
    HtmlString += "<BR><A href='/'>Главная</A></BR>";
    HtmlString += "<BR><A href='/ownersearch'>Поиск владельцев</A></BR>";
    HtmlString += "<BR><A href='/carsearch'>Поиск по машинам</A></BR>";
    HtmlString += "<BR><A href='/cars'>Машины</A></BR>";
    HtmlString += "<BR><A href='/owners'>Владельцы</A></BR>";
    HtmlString += "<BR><A href='/info'>Информация о клиенте</A></BR>";
    HtmlString += "</BODY></HTML>";
    return context.Response.WriteAsync(HtmlString);
});


app.Run();