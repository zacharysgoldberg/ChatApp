﻿using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BuggyController: BaseApiController
{
    private readonly ApplicationDbContext _context;
    public BuggyController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetSecret()
    {
        return "secret text";
    }

    [HttpGet("not-found")]
    public ActionResult<AppUser> GetNotFound()
    {
        AppUser thing = _context.Users.Find(-1);
        
        if(thing is null)
            return NotFound();
        return thing;
    }

    [HttpGet("server-error")]
    public ActionResult<string> GetServerError()
    {
        AppUser thing = _context.Users.Find(-1);
        
        string thingToReturn = thing.ToString();

        return thingToReturn;
    }

     [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest()
    {
       return BadRequest("This was not a good request");
    }
}
