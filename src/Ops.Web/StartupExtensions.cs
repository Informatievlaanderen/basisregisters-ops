﻿namespace Ops.Web;

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class StartupExtensions
{
    public static WebApplicationBuilder AddOptions<TOptions>(this WebApplicationBuilder builder)
        where TOptions : class
    {
        builder.Services.Configure<TOptions>(builder.Configuration.GetSection(typeof(TOptions).Name));
        return builder;
    }

    public static TOptions GetAppOptions<TOptions>(this WebApplicationBuilder builder)
        where TOptions : class, new()
    {
        var options = new TOptions();
        builder.Configuration.Bind(typeof(TOptions).Name, options);

        var requiredProperties = options
            .GetType()
            .GetProperties()
            .Where(prop => Attribute.IsDefined(prop, typeof(RequiredAttribute)));

        void ThrowArgumentNullException(PropertyInfo p) => throw new ArgumentNullException($"{typeof(TOptions).Name}.{p.Name}");

        foreach (var prop in requiredProperties)
        {
            var obj = prop.GetValue(options, null);

            if (prop.GetValue(options, null) is null)
            {
                ThrowArgumentNullException(prop);
            }

            if (obj is string valstr && string.IsNullOrEmpty(valstr))
            {
                ThrowArgumentNullException(prop);
            }
        }

        return options;
    }
}
