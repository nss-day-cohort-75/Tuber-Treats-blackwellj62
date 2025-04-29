using TuberTreats.Models;
using TuberTreats.Models.DTOs;

List<TuberDriver> drivers = new List<TuberDriver>
{
    new TuberDriver
    {
        Id = 1,
        Name = "Fat Perez"
    },
    new TuberDriver
    {
        Id = 2,
        Name = "Bobby Fairways"
    },
    new TuberDriver
    {
        Id = 3,
        Name = "Peter D"
    }
};
List<Customer> customers = new List<Customer>
{
    new Customer
    {
        Id = 1,
        Name = "The Tick",
        Address = "Par 3 Blvd."
    },
    new Customer
    {
        Id = 2,
        Name = "Ben Yammin",
        Address = "18 Shallow Dr."
    },
    new Customer
    {
        Id = 3,
        Name = "Wilbur",
        Address = "8va Wildcat Way"
    },
    new Customer
    {
        Id = 4,
        Name = "MARTON",
        Address = "2 Puzzle St."
    },
    new Customer
    {
        Id = 5,
        Name = "TJ",
        Address = "6241 Triangle Dr."
    }
};
List<Topping> toppings = new List<Topping>
{
    new Topping
    {
       Id = 1,
       Name = "Bacon" 
    },
    new Topping
    {
       Id = 2,
       Name = "Sour Cream" 
    },
    new Topping
    {
       Id = 3,
       Name = "Chives"
    },
    new Topping
    {
       Id = 4,
       Name = "Cheese"
    },
    new Topping
    {
       Id = 5,
       Name = "Chili"
    }
};
List<TuberOrder> orders = new List<TuberOrder>
{
    new TuberOrder
    {
        Id = 1,
        OrderPlacedOnDate =  new DateTime(2025,4,2),
        CustomerId = 1,
        TuberDriverId = 1,
        DeliveredOnDate = new DateTime(2025,4,2),

    },
    new TuberOrder
    {
        Id = 2,
        OrderPlacedOnDate =  new DateTime(2025,4,5),
        CustomerId = 2,
        TuberDriverId = 2,
        DeliveredOnDate = new DateTime(2025,4,5),
        
    },
    new TuberOrder
    {
        Id = 3,
        OrderPlacedOnDate =  new DateTime(2025,4,26),
        CustomerId = 4,
        TuberDriverId = null,
        DeliveredOnDate = null
        
    }
};
List<TuberTopping> tuberToppings = new List<TuberTopping>
{
    new TuberTopping
    {
        Id = 1,
        TuberOrderId = 1,
        ToppingId = 5
    },
    new TuberTopping
    {
        Id = 2,
        TuberOrderId = 1,
        ToppingId = 4
    },
    new TuberTopping
    {
        Id = 3,
        TuberOrderId = 2,
        ToppingId = 2
    },
    new TuberTopping
    {
        Id = 4,
        TuberOrderId = 2,
        ToppingId = 3
    }
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//add endpoints here

app.MapGet("/tuberorders", ()=>{
    return orders.Select(o => 
    {
    List<TuberTopping> joinTable = tuberToppings.Where(tt=> tt.TuberOrderId == o.Id).ToList();
    List<Topping> topping = joinTable.Select(jt => toppings.FirstOrDefault(t => t.Id == jt.ToppingId)).ToList(); 

    Customer customer = customers.FirstOrDefault(cs => cs.Id == o.CustomerId);
    TuberDriver tuberDriver = drivers.FirstOrDefault(d => d.Id == o.TuberDriverId);

    
    return new TuberOrderDTO
    {
        Id = o.Id,
        OrderPlacedOnDate = o.OrderPlacedOnDate,
        CustomerId = o.CustomerId,
        TuberDriverId = o.TuberDriverId,
        DeliveredOnDate = o.DeliveredOnDate,
        Toppings= topping.Select(t => new ToppingDTO
        {
            Id = t.Id,
            Name = t.Name
        }).ToList(),
        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        TuberDriver = tuberDriver == null ? null : new TuberDriverDTO
        {
            Id = tuberDriver.Id,
            Name = tuberDriver.Name
        }
    };

    });
});

app.MapGet("/tuberorders/{id}", (int id)=>
{
    TuberOrder tuberOrder = orders.FirstOrDefault(o => o.Id == id);
    if (tuberOrder == null)
    {
        return Results.NotFound();
    }
    Customer customer = customers.FirstOrDefault(customer => customer.Id == tuberOrder.CustomerId);
    TuberDriver tuberDriver = drivers.FirstOrDefault(driver => driver.Id == tuberOrder.TuberDriverId);

    List<TuberTopping> toppingsTables = tuberToppings.Where(tt => tt.TuberOrderId == tuberOrder.Id).ToList();
    List<Topping> topping = toppingsTables.Select(tt => toppings.FirstOrDefault(t => t.Id == tt.ToppingId)).ToList();

    return Results.Ok (
        new TuberOrderDTO
        {
           Id = tuberOrder.Id,
        OrderPlacedOnDate = tuberOrder.OrderPlacedOnDate,
        CustomerId = tuberOrder.CustomerId,
        TuberDriverId = tuberOrder.TuberDriverId,
        DeliveredOnDate = tuberOrder.DeliveredOnDate,
        Toppings= topping.Select(t => new ToppingDTO
        {
            Id = t.Id,
            Name = t.Name
        }).ToList(),
        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        TuberDriver = tuberDriver == null ? null : new TuberDriverDTO
        {
            Id = tuberDriver.Id,
            Name = tuberDriver.Name
        }  
        }
    );
});

app.MapPost("/tuberorders", (TuberOrder tuberOrder) =>
{
    Customer customer = customers.FirstOrDefault(c=> c.Id == tuberOrder.CustomerId);

    if (tuberOrder.CustomerId == 0 || customer == null)
    {
        return Results.BadRequest();
    }
    tuberOrder.Id = orders.Max(tuberOrder => tuberOrder.Id) + 1;
    tuberOrder.OrderPlacedOnDate = DateTime.Now;
    orders.Add(tuberOrder);
    return Results.Created($"/tuberorders/{tuberOrder.Id}", new TuberOrderDTO
    {
        Id = tuberOrder.Id,
        OrderPlacedOnDate = tuberOrder.OrderPlacedOnDate,
        CustomerId = tuberOrder.CustomerId,
        TuberDriverId = tuberOrder.TuberDriverId,
        DeliveredOnDate = tuberOrder.DeliveredOnDate,
        Toppings = null,
        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        TuberDriver = null
    });
});

app.MapPut("/tuberorders/{id}", (int id, TuberOrder tuberOrder)=>
{
    TuberDriver tuberDriver = drivers.FirstOrDefault(d => d.Id == tuberOrder.TuberDriverId);
    TuberOrder orderToUpdate = orders.FirstOrDefault(o => o.Id == id);
    if ( orderToUpdate == null)
    {
        return Results.NotFound();
    }
    if (id != tuberOrder.Id)
    {
        return Results.BadRequest();
    }

    orderToUpdate.TuberDriverId = tuberOrder.TuberDriverId;
    return Results.Ok( new TuberOrderDTO
    {
        Id = orderToUpdate.Id,
        OrderPlacedOnDate = orderToUpdate.OrderPlacedOnDate,
        CustomerId = orderToUpdate.CustomerId,
        TuberDriverId = orderToUpdate.TuberDriverId,
        DeliveredOnDate = orderToUpdate.DeliveredOnDate,
        TuberDriver = tuberDriver == null ? null : new TuberDriverDTO
        {
            Id = tuberDriver.Id,
            Name = tuberDriver.Name
        }
    });

    
});

app.MapPost("/tuberorders/{id}/complete", (int id)=>
{
    TuberOrder orderToComplete = orders.FirstOrDefault(o => o.Id == id);
    if (orderToComplete == null)
    {
        return Results.NotFound();
    }
    orderToComplete.DeliveredOnDate = DateTime.Now;

    Customer customer = customers.FirstOrDefault(cs => cs.Id == orderToComplete.CustomerId);
    TuberDriver tuberDriver = drivers.FirstOrDefault(d => d.Id == orderToComplete.TuberDriverId);

    List<TuberTopping> toppingsTables = tuberToppings.Where(tt => tt.TuberOrderId == orderToComplete.Id).ToList();
    List<Topping> topping = toppingsTables.Select(tt => toppings.FirstOrDefault(t => t.Id == tt.ToppingId)).ToList();

    return Results.Ok( new TuberOrderDTO 
    {
        Id = orderToComplete.Id,
        OrderPlacedOnDate = orderToComplete.OrderPlacedOnDate,
        CustomerId = orderToComplete.CustomerId,
        TuberDriverId = orderToComplete.TuberDriverId,
        DeliveredOnDate = orderToComplete.DeliveredOnDate,
        Toppings = topping.Select(t => new ToppingDTO
        {
            Id = t.Id,
            Name = t.Name
        }).ToList(),
        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        TuberDriver = tuberDriver == null ? null : new TuberDriverDTO
        {
            Id = tuberDriver.Id,
            Name = tuberDriver.Name
        }
    });
});

app.MapGet("/toppings", ()=>
{
    return toppings.Select( t => new ToppingDTO
    {
        Id = t.Id,
        Name = t.Name
    });
});

app.MapGet("/toppings/{id}", (int id)=>
{
    Topping topping = toppings.FirstOrDefault(t => t.Id == id);
    if (topping == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(new ToppingDTO
    {
        Id = topping.Id,
        Name = topping.Name
    });
});

app.MapGet("/tubertoppings",()=>
{
    return tuberToppings.Select(tt => new TuberToppingDTO
    {
        Id = tt.Id,
        TuberOrderId = tt.TuberOrderId,
        ToppingId = tt.ToppingId
    });
});

app.MapPost("/tubertoppings",(TuberTopping topping)=>
{
    if (topping.Id == 0 || topping.TuberOrderId == 0 || topping.ToppingId == 0)
    {
        return Results.BadRequest();
    }
    topping.Id = tuberToppings.Max(t => t.Id) + 1;
    tuberToppings.Add(topping);
    return Results.Created($"/toppings{topping.Id}", new TuberToppingDTO
    {
        Id = topping.Id,
        ToppingId = topping.ToppingId,
        TuberOrderId = topping.TuberOrderId
    });
});

app.MapDelete("/tubertoppings/{id}",(int id)=>
{
    TuberTopping tuberTopping = tuberToppings.FirstOrDefault(tt => tt.Id == id);
    if (tuberTopping == null)
    {
        return Results.NotFound();
    }
    tuberToppings.Remove(tuberTopping);
    return Results.NoContent();
});

app.MapGet("/customers", ()=>
{
    return customers.Select(cs => new CustomerDTO
    {
        Id = cs.Id,
        Name = cs.Name,
        Address = cs.Address
    });
});

app.MapGet("/customers/{id}",(int id)=>
{
   Customer customer = customers.FirstOrDefault(cs => cs.Id == id); 
   List<TuberOrder> tuberOrders = orders.Where(o => o.CustomerId == customer.Id).ToList();

   if (customer == null)
   {
    return Results.NotFound();
   }
   return Results.Ok(new CustomerDTO
   {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        TuberOrders = tuberOrders.Where(to => to.CustomerId == customer.Id).ToList()
   });

});

app.MapPost("/customers", (Customer customer)=>
{
    if (customer.Name == null || customer.Address == null)
    {
        return Results.BadRequest();
    }
    customer.Id = customers.Max(c => c.Id) + 1;
    customers.Add(customer);
    return Results.Created($"/customers/{customer.Id}", new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address
    });
});

app.MapDelete("/customers/{id}", (int id)=>
{
    Customer customer = customers.FirstOrDefault(cs => cs.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(
    customers.Remove(customer)
    );
});

app.MapGet("/tuberdrivers", ()=>
{
    return drivers.Select(d => new TuberDriverDTO
    {
        Id = d.Id,
        Name= d.Name
    });
});

app.MapGet("/tuberdrivers/{id}", (int id)=>
{
    TuberDriver tuberDriver = drivers.FirstOrDefault(d => d.Id == id);
    if (tuberDriver == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(new TuberDriverDTO
    {
        Id = tuberDriver.Id,
        Name = tuberDriver.Name,
        TuberDeliveries =  orders.Where(o => o.TuberDriverId == tuberDriver.Id).ToList()
    }
    );
});
app.Run();
//don't touch or move this!
public partial class Program { }