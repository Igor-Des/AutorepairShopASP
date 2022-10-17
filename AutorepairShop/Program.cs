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
        // ������������ ������ ��� ������ 
        string strResponse = "<HTML><HEAD><TITLE>����������</TITLE></HEAD>" +
        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
        "<BODY><H1>����������:</H1>";
        strResponse += "<BR> ������: " + context.Request.Host;
        strResponse += "<BR> ����: " + context.Request.PathBase;
        strResponse += "<BR> ��������: " + context.Request.Protocol;
        strResponse += "<BR><A href='/'>�������</A></BODY></HTML>";
        // ����� ������
        await context.Response.WriteAsync(strResponse);
    });
});

app.Map("/owners", (appBuilder) =>
{
    appBuilder.Run(async (context) =>
    {
        ICachedOwnersService cachedOwner = context.RequestServices.GetService<ICachedOwnersService>();
        IEnumerable<Owner> owners = cachedOwner.GetOwners("Wilfred");
        string HtmlString = "<HTML><HEAD><TITLE>������</TITLE></HEAD>" +
        "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
        "<BODY><H1>������ ��������</H1>" +
        "<TABLE BORDER=1>";
        HtmlString += "<TR>";
        HtmlString += "<TH>��� �������</TH>";
        HtmlString += "<TH>��� �������</TH>";
        HtmlString += "<TH>����� �������</TH>";
        HtmlString += "<TH>������� �������</TH>";
        HtmlString += "<TH>������ �������</TH>";
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
        HtmlString += "<BR><A href='/'>�������</A></BR>";
        HtmlString += "</BODY></HTML>";
        await context.Response.WriteAsync(HtmlString);
    });
});


app.MapGet("/", (context) =>
{
    ICachedCarsService cachedCar = context.RequestServices.GetService<ICachedCarsService>();
    //cachedCar.AddCars("BMW");
    ICachedOwnersService cachedOwner = context.RequestServices.GetService<ICachedOwnersService>();
    //cachedOwner.AddOwners("OwnerName");
    string HtmlString = "<HTML><HEAD><TITLE>autorepar shop asp net</TITLE></HEAD>" +
    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
    "<BODY><H1>�������</H1>";
    HtmlString += "<H2>������ �������� � ��� �������</H2>";
    HtmlString += "<BR><A href='/'>�������</A></BR>";
    HtmlString += "<BR><A href='/'>����� �� �������</A></BR>";
    HtmlString += "<BR><A href='/'>����� �� ��������</A></BR>";
    HtmlString += "<BR><A href='/'>����������</A></BR>";
    HtmlString += "<BR><A href='/owners'>���������</A></BR>";
    HtmlString += "<BR><A href='/info'>������ � �������</A></BR>";
    HtmlString += "</BODY></HTML>";
    return context.Response.WriteAsync(HtmlString);
});


app.Run();