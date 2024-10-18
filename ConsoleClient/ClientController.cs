﻿using System.Net;
using System.Text.Json;
using ConsoleClient.Services.Abstractions;
using ConsoleClient.Services.DTOs;
using Spectre.Console;

namespace ConsoleClient;

public class ClientController
{
    private readonly IBase64Encoder _base64Encoder;
    private readonly IUserService _userService;

    public ClientController(IBase64Encoder base64Encoder, IUserService userService)
    {
        _base64Encoder = base64Encoder;
        _userService = userService;
    }

    public void Run()
    {
        while (true)
        {
            (int, string) input = AnsiConsole.Prompt(new SelectionPrompt<(int, string)>()
                .Title("Select an action")
                .UseConverter(tuple => $"{tuple.Item1} - {tuple.Item2}")
                .AddChoices(new []
                {
                    (0, "Create user"),
                    (1, "Get users"),
                    (2, "Get user by id"),
                    (3, "Update user"),
                    (4, "Delete user"),
                }));

            AnsiConsole.Clear();

            switch (input.Item1)
            {
                case 0:
                    CreateUser(); 
                    break;
                case 1:
                    GetUsers();
                    break;
                case 2:
                    GetUserById();
                    break;
                case 3:
                    break;
                case 4:
                    break;
            }
        }
    }

    private void CreateUser()
    {
        Tuple<string, string> input = InputAuthData();
        
        string passwordRepeat = AnsiConsole.Prompt(new TextPrompt<string>("Repeat password:")
            .Validate(s =>
            {
                if (s.Length < 1)
                {
                    return ValidationResult.Error("Too short");
                }
                if (s.Contains(':') || s.Contains(' '))
                {
                    return ValidationResult.Error("Invalid characters");
                }
                
                return ValidationResult.Success();
            }));

        if (input.Item2 != passwordRepeat)
        {
            AnsiConsole.MarkupLine("[red]Passwords do not match![/]");
            return;
        }
        
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Sending request...", _ =>
            {
                try
                {
                    CreateUserDto a = new CreateUserDto(input.Item1, input.Item2);
                    int id = _userService.Create(a);
                    if (id < 0)
                    {
                        throw new Exception("Failed to create user");
                    }

                    AnsiConsole.MarkupLine($"[green]Success! Created user id: {id}[/]");
                }
                catch (HttpRequestException e)
                {
                    AnsiConsole.MarkupLine($"[red]Error! {e.StatusCode}[/]");
                }
                catch (Exception e)
                {
                    AnsiConsole.MarkupLine($"[red]Error! {e.Message}[/]");
                }
            });
    }

    private void GetUsers()
    {
        AnsiConsole.MarkupLine("Authorize");
        Tuple<string, string> input = InputAuthData();
        string basicAuthToken = _base64Encoder.Encode(input.Item1 + ":" + input.Item2);
        
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Aesthetic)
            .Start("Sending request...", ctx =>
            {
                try
                {
                    IDictionary<int, UserDto> users = _userService.GetUsers(basicAuthToken);
                    AnsiConsole.MarkupLine("[green]Success![/]");
                    AnsiConsole.MarkupLine("Users:");
                    foreach (KeyValuePair<int, UserDto> idUserPair in users)
                    {
                        Thread.Sleep(250);
                        AnsiConsole.MarkupLine($"> {idUserPair.Key} - {idUserPair.Value}");
                    }
                }
                catch (HttpRequestException e)
                {
                    AnsiConsole.MarkupLine($"[red]Error! {e.StatusCode}[/]");
                }
                catch (Exception e)
                {
                    AnsiConsole.MarkupLine($"[red]Error! {e.Message }[/]");
                }
            });
    }

    private void GetUserById()
    {
        int usernameId = AnsiConsole.Prompt(new TextPrompt<int>("Username id:")
            .Validate(id =>
            {
                if (id < 1)
                {
                    return ValidationResult.Error("Too short");
                }
                
                return ValidationResult.Success();
            }));
        
        AnsiConsole.MarkupLine("Authorize");
        Tuple<string, string> input = InputAuthData();
        string basicAuthToken = _base64Encoder.Encode(input.Item1 + ":" + input.Item2);
        
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Aesthetic)
            .Start("Sending request...", ctx =>
            {
                try
                {
                    UserDto? user = _userService.GetUserById(basicAuthToken, usernameId);
                    if (user is not null)
                    {
                        AnsiConsole.MarkupLine($"[green]Success! User: {user.Username}[/]");
                    }
                }
                catch (HttpRequestException e)
                {
                    AnsiConsole.MarkupLine($"[red]Error! {e.StatusCode}[/]");
                }
            });
    }

    private Tuple<string,string> InputAuthData()
    {
        string username = AnsiConsole.Prompt(new TextPrompt<string>("Username:")
            .Validate(s =>
            {
                if (s.Length < 1)
                {
                    return ValidationResult.Error("Too short");
                }
                if (s.Contains(':') || s.Contains(' '))
                {
                    return ValidationResult.Error("Invalid characters");
                }
                
                return ValidationResult.Success();
            }));

        string password = AnsiConsole.Prompt(new TextPrompt<string>("Password:")
            .Secret('*')
            .Validate(s =>
            {
                if (s.Length < 1)
                {
                    return ValidationResult.Error("Too short");
                }
                if (s.Contains(':') || s.Contains(' '))
                {
                    return ValidationResult.Error("Invalid characters");
                }
                
                return ValidationResult.Success();
            }));

        return new Tuple<string, string>(username, password);
    }
}