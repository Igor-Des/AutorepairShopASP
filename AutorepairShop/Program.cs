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

builder.Services.AddScoped<ICachedOwnersService, CachedOwnersService>();
builder.Services.AddScoped<ICachedCarsService, CachedCarsService>();
builder.Services.AddScoped<ICachedPaymentsService, CachedPaymentsService>();


var app = builder.Build();



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
        HtmlString += "<TH>Имя владельца</TH>";
        HtmlString += "<TH>Фамилия владельца</TH>";
        HtmlString += "<TH>Отчество владельца</TH>";
        HtmlString += "<TH>Телефон владельца</TH>";
        HtmlString += "<TH>Адрес владельца</TH>";
        HtmlString += "<TH>MORE PROP</TH>";
        HtmlString += "</TR>";
        foreach (Owner owner in owners)
        {
            HtmlString += "<TR>";
            HtmlString += "<TD>" + owner.FirstName + "</TD>";
            HtmlString += "<TD>" + owner.MiddleName + "</TD>";
            HtmlString += "<TD>" + owner.LastName + "</TD>";
            HtmlString += "<TD>" + owner.Phone + "</TD>";
            HtmlString += "<TD>" + owner.Address + "</TD>";
            HtmlString += "<TD>" + " AND MORE PROP " + "</TD>";
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
        IEnumerable<Car> cars = cachedCars.GetCars(40);
        string HtmlString = "<HTML><HEAD><TITLE>Car</TITLE></HEAD>" +
        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
        "<BODY><H1>Список автомобилей</H1>" +
        "<TABLE BORDER=0>";
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

app.Map("/carsearch", (appBuider) =>
{
    appBuider.Run(async (context) =>
    {
        ICachedCarsService cachedCars = context.RequestServices.GetService<ICachedCarsService>();
        IEnumerable<Car> cars = cachedCars.GetCars("BMW", 40);
        string brandStr;
        if (context.Request.Cookies.TryGetValue("brand", out brandStr)) { }
        string strResponse = "<HTML><HEAD><TITLE>Cars search</TITLE></HEAD>" +
        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
        "<BODY><FORM action ='/carsearch' / >" +
        "Имя:<BR><INPUT type = 'text' name = 'Brand' value = " + brandStr + ">" +
        "<BR><BR><INPUT type ='submit' value='Сохранить в куки и показать'></FORM>" +
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


//Запоминание в Session значений, введенных в форме
app.Map("/ownersearch", (appBuilder) =>
{
    appBuilder.Run(async (context) =>
    {
        // надо будет дописать
    });
});


app.MapGet("/", (context) =>
{
    ICachedCarsService cachedCar = context.RequestServices.GetService<ICachedCarsService>();
    //cachedCar.AddCars("BMW");
    ICachedOwnersService cachedOwner = context.RequestServices.GetService<ICachedOwnersService>();
    //cachedOwner.AddOwners("OwnerName");
    string HtmlString = "<HTML><HEAD><TITLE>autorepair shop</TITLE></HEAD>" +
                "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                "<BODY><H1>Главная</H1>";
    HtmlString += "<H2>Данные записаны в кэш сервера</H2>";
    HtmlString += "<BR><A href='/'>Главная</A></BR>";
    HtmlString += "<BR><A href='/ownersearch'>Поиск по владельцам</A></BR>";
    HtmlString += "<BR><A href='/carsearch'>Поиск по машинам</A></BR>";
    HtmlString += "<BR><A href='/cars'>Cars</A></BR>";
    HtmlString += "<BR><A href='/owners'>Owners</A></BR>";
    HtmlString += "<BR><A href='/info'>about sever</A></BR>";
    HtmlString += "</BODY></HTML>";
    return context.Response.WriteAsync(HtmlString);
});


app.Run();