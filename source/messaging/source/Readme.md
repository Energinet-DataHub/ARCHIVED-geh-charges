# Message ingest

## General

This solution serves as the basis for the message ingestion processing part of the Green Energy Hub, and proves out the ability to have a single entry point (could be an Azure Function) that is able to accept different requests types and logically route them to classes fulfilling the respective business processes for that request.

It provides an opinionated framework that self-registers classes to make adding new requests, handlers, and rule sets easy, and currently uses FluentValidation as the rules framework.

It serves HTTP endpoints for each of the discovered request types, and if validation succeeds, pushes the accepted messages onto a queue.

## Concepts

Core concepts for customizing the request processing flow are as follows:

* *Hub Action Requests*: A small POCO-like class implementing `IHubActionRequest` that defining the properties expected on a request of that type.
* *Hub Action Handlers*: Provides the business processing logic for handling a request of that type, namely validating the message against business rules. Must derive from `IngestionHandler<HubActionRequestType>` and handles any validation processes in `ValidateAsync()`, persists the request for further processing in `AcceptAsync()`, and crafts a `HubActionResponse` to return the caller in `RespondAsync()`.
* *RuleCollection*: Serves as a base class for defining validation rules for a DTO.
* *PropertyRule&lt;T&gt;*: Provides custom validation logic for a property that can be reused in RuleCollections.

## Requests and handlers

To implement logic for a new request, classes for each of the types above should be created. `HubMessageAttribute` can be used to create a friendly name for a concrete hub action request type.

All of the classes implementing `IHubActionRequest` and `IHubActionHandler<HubActionRequestType>` are auto-discovered and registered via `AddGreenEnergyHub()` service extension in the `HandlerExtensions` class, permitting requests of that type to flow to the correct handler automatically when called at `http://localhost:7071/api/HubActionRequestType`. `MessageRegistration` helps with this auto-registration.

## Rules and rule sets

### Validation logic

The validation library uses FluentValidation to perform the property validations. To illustrate the use of the framework the samples will use the following classes.

```csharp
public class ChangeSupplier {
    public MarketEvaluationPoint MarketEvaluationPoint { get; set; }
}

public class MoveIn {
    public MarketEvaluationPoint MarketEvaluationPoint { get; set; }
}

public class MarketEvaluationPoint : ValueObject {
    public string GSRN { get; set; }
}
```

A rule could be that the value of `MarketEvaluationPoint.GSRN` should be 18 digits long. This rule could be implemented as.

```csharp
public class MarketEvaluationPointGsrnShouldBe18DigitsLong : PropertyRule<string> {
    protected internal override string Code { get; } = "VR.001";

    protected override bool IsValid(string propertyValue, PropertyValidatorContext context)
    {
        if (propertyValue == null) return false;
        return propertyValue.Length == 18;
    }
}
```

To use this new class a RuleCollection for `MarketEvaluationPoint` is created.

``` csharp
public class MarketEvaluationPointRuleCollection : RuleCollection<MarketEvaluationPoint> {
    public MarketEvaluationPointRuleCollection() {
        RuleFor(p => p.GSRN)
            .PropertyRule<MarketEvaluationPointGsrnShouldBe18DigitsLong>();
    }
}
```

Using this new class in the `ChangeSupplier` a corresponding `RuleCollection` is implemented.

``` csharp
public class ChangeSupplierRuleCollection : RuleCollection<ChangeSupplier> {
    public ChangeSupplierRuleCollection() {
        RuleFor(p => p.MarketEvaluationPoint)
            .RuleCollection<MarketEvaluationPointRuleCollection>();
    }
}
```

We can then reuse the same rule for `MoveIn`.

``` csharp
public class MoveInRuleCollection : RuleCollection<MoveIn> {
    public MoveInRuleCollection() {
        RuleFor(p => p.MarketEvaluationPoint)
            .RuleCollection<MarketEvaluationPointRuleCollection>();
    }
}
```

When the actual rule is used, the class that needs to perform the validation takes a dependency on `IRuleEngine<T>` where T is the type that needs to be validated.

Fx.:

``` csharp
public class ChangeOfSupplierWorkflow {

    private readonly IRuleEngine<ChangeSupplier> _validator;

    public ChangeOfShupplierWorkflow(IRuleEngine<ChangeSupplier> inputValidation) {

        _validator = inputValidation;
    }

    public async Task ProcessAsync(ChangeSupplier changeSupplier) {

        var result = await _validator.ValidateAsync(changeSupplier);
    }
}
```

The variable `result` contains the output of the validation and can be acted upon.

When the method `AddGreenEnergyHub(...)` is invoked in startup all classes are found based on assembly scanning. There is no need to add separate registrations for `RuleCollection`.

### Generics and rule matching

All rules declarations must be generic (`where TRequest : IActionRequest`) because NRules matching happens against fact *types*. While matching the fact against the common interface `IActionRequest` will cause rules to execute, the validation cannot access any properties properties on the object; using generics, the `TRequest` can represent the expected and specific shape of the request and if we constrain `TRequest` using property-based interfaces, we can also guarantee their presence at compile time.

Thus, the solution defines interfaces for various properties available on requests; for example `IHubMessageHasCustomerId` defines a single property `CustomerId`. A rule that requires `CustomerId` would then constrain its `where TRequest : IActionRequest, IHubMessageHasCustomerId` to expose the property for validation in the rule action.

Rule sets bind a group of `Rule`s to a particular request type by implementing `IRuleSet<HubActionRequestType>`. However, type references to rules in their static enumeration should be kept generic (i.e. `typeof(NonNegativeCustomerIdRule<>)`) so that the rule set can be composable; for example one may wish to create a "Customer" rule set that applies a set of validations to any request that references a `CustomerId` (via `IHubMessageHasCustomerId`).

As such, having rule sets listing generic `Rule` instances permits for composing rule sets at compile time and binding the `Rule` instances to the correct `TRequest` at runtime (which happens in the `NRulesEngine` class).
