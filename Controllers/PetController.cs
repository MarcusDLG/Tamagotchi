using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tamagotchi.Models;
using System.Collections.Generic;

namespace Tamagotchi.Controllers
{
  [Route("api/[controller]")]
  [ApiController]

  public class PetController : ControllerBase
  {
    public DatabaseContext db { get; set; } = new DatabaseContext();

    [HttpGet]
    public List<Pet> GetAllPets()
    {

      var pets = db.Pets.OrderBy(n => n.Name);
      KillPets(pets.ToList());

      return pets.ToList();
    }

    [HttpGet("{id}")]
    public Pet GetOnePet(int id)
    {
      var pet = db.Pets.FirstOrDefault(i => i.Id == id);
      InteractionUpdate(pet);
      return pet;
    }

    [HttpGet("alive")]
    public List<Pet> ViewAlive()
    {
      var alive = db.Pets.Where(i => i.IsDead == false);
      var ordered = alive.OrderBy(n => n.Id);
      return ordered.ToList();
    }

    [HttpPost]
    public Pet CreateNewTamagotchi(Pet pet)
    {
      db.Pets.Add(pet);
      db.SaveChanges();
      return pet;
    }

    [HttpPatch("{id}/play")]
    public ActionResult<Pet> PlayWithPet(int id)
    {
      var petToChange = db.Pets.FirstOrDefault(i => i.Id == id);
      var killed = RandomKilling();
      if (petToChange.IsDead == true)
      {
        return NotFound();
      }
      else if (petToChange.IsDead == false)
      {
        if (killed == true)
        {
          petToChange.IsDead = true;
          petToChange.DeathDate = DateTime.Now;
        }
        else if (killed == false)
        {
          petToChange.HappinessLevel = petToChange.HappinessLevel + 5;
          petToChange.HungerLevel = petToChange.HungerLevel + 3;
        }

      }
      InteractionUpdate(petToChange);
      return petToChange;
    }

    [HttpPatch("{id}/feed")]
    public ActionResult<Pet> FeedPet(int id)
    {
      var petToFeed = db.Pets.FirstOrDefault(i => i.Id == id);
      if (petToFeed.IsDead == true)
      {
        return NotFound();
      }
      else if (petToFeed.IsDead == false)
      {
        var killed = RandomKilling();
        if (killed == true)
        {
          petToFeed.IsDead = true;
          petToFeed.DeathDate = DateTime.Now;
        }
        else if (killed == false)
        {
          petToFeed.HappinessLevel = petToFeed.HappinessLevel + 3;
          petToFeed.HungerLevel = petToFeed.HungerLevel - 5;
        }
      }
      InteractionUpdate(petToFeed);
      return petToFeed;
    }

    [HttpPatch("{id}/scold")]
    public ActionResult<Pet> ScoldPet(int id)
    {
      var petToScold = db.Pets.FirstOrDefault(i => i.Id == id);
      if (petToScold.IsDead == true)
      {
        return NotFound();
        // return petToScold;
      }
      else if (petToScold.IsDead == false)
      {

        var killed = RandomKilling();
        if (killed == true)
        {
          petToScold.IsDead = true;
          petToScold.DeathDate = DateTime.Now;
        }
        else if (killed == false)
        {
          petToScold.HappinessLevel = petToScold.HappinessLevel - 5;
        }
      }
      InteractionUpdate(petToScold);
      return petToScold;
    }

    [HttpDelete("{id}")]
    public ActionResult PetToDelete(int id)
    {
      var petToRemove = db.Pets.FirstOrDefault(f => f.Id == id);
      if (petToRemove == null)
      {
        return NotFound();
      }
      db.Pets.Remove(petToRemove);
      db.SaveChanges();
      return Ok();
    }

    public bool RandomKilling()
    {
      var killed = false;
      var sudden = new Random();
      var dead = sudden.Next(1, 101);
      if (dead <= 10)
      {
        killed = true;
      }
      return killed;

    }
    public void InteractionUpdate(Pet pet)
    {
      pet.LastInteractedWithDate = DateTime.Now;
      db.SaveChanges();
    }
    public void KillPets(List<Pet> pets)
    {
      foreach (var p in pets)
      {
        if (p.IsDead == false)
        {

          if (p.LastInteractedWithDate < DateTime.Now.AddDays(-3))
          {
            p.IsDead = true;
          }
          else if (p.LastInteractedWithDate > DateTime.Now.AddDays(-3))
          {
            p.IsDead = false;
          }
        }
      }
    }
  }
}