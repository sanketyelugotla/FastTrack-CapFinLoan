# Complete Guide to Angular and .NET Microservices Architecture

**Date:** April 2026  
**Purpose:** Comprehensive reference for Angular development, .NET microservices, and enterprise architecture patterns

---

## Table of Contents

1. [Angular Core Concepts](#angular-core-concepts)
2. [Dependency Injection (DI)](#dependency-injection-di)
3. [Directives](#directives)
4. [Components & Lifecycle](#components--lifecycle)
5. [Reactive Programming & RxJS](#reactive-programming--rxjs)
6. [.NET Microservices Architecture](#net-microservices-architecture)
7. [3-Layer Pattern](#3-layer-pattern)
8. [Web API in .NET](#web-api-in-net)
9. [Message Queuing with RabbitMQ](#message-queuing-with-rabbitmq)
10. [Saga Pattern](#saga-pattern)
11. [Best Practices](#best-practices)

---

## Angular Core Concepts

### What is Angular?

Angular is a TypeScript-based open-source web application framework maintained by Google. It provides a complete solution for building single-page applications (SPAs) with:
- Component-based architecture
- Two-way data binding (older versions) / Reactive approach (modern)
- Built-in HTTP client
- Routing system
- Form validation
- Testing utilities

### Angular Versions

- **AngularJS (1.x):** Original framework, now deprecated
- **Angular 2-5:** Complete rewrite in TypeScript, major version releases
- **Angular 6+:** Semantic versioning, minor versions released every 6 months
- **Angular 14+:** Standalone components introduced
- **Angular 17+:** Signals API (reactive primitive), better type safety

### Key Architectural Concepts

#### Module System (Pre-Angular 14)
```typescript
// Traditional approach using NgModules
@NgModule({
  declarations: [AppComponent, MyComponent],
  imports: [BrowserModule, FormsModule],
  providers: [MyService],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

#### Standalone Components (Angular 14+)
```typescript
// Modern approach - no NgModule needed
@Component({
  selector: 'app-my',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `<h1>{{ title }}</h1>`,
})
export class MyComponent {
  title = 'Standalone Component';
}
```

### Angular Project Structure

```
my-app/
├── src/
│   ├── app/
│   │   ├── components/       # Reusable components
│   │   ├── services/         # Business logic
│   │   ├── models/           # Interfaces/types
│   │   ├── guards/           # Route guards
│   │   ├── interceptors/      # HTTP interceptors
│   │   ├── app.component.ts
│   │   └── app.routes.ts      # Routing config
│   ├── assets/               # Static files
│   ├── environments/         # Environment configs
│   ├── main.ts               # Entry point
│   └── index.html
├── angular.json              # Angular config
├── tsconfig.json             # TypeScript config
├── package.json
└── README.md
```

---

## Dependency Injection (DI)

### What is Dependency Injection?

Dependency Injection is a design pattern that deals with how components get hold of their dependencies. It promotes loose coupling by allowing dependencies to be provided externally.

### Benefits of DI

- **Testability:** Easy to mock dependencies in tests
- **Maintainability:** Changes to dependencies don't affect consumers
- **Reusability:** Services can be easily shared across components
- **Flexibility:** Easy to swap implementations
- **Scalability:** Better code organization

### Angular Injector

The Angular Injector is responsible for creating instances of services and resolving dependencies.

```typescript
// Service definition
@Injectable({
  providedIn: 'root'  // Makes it a singleton available app-wide
})
export class UserService {
  constructor(private http: HttpClient) {}
}

// Component using DI
@Component({
  selector: 'app-user',
  standalone: true,
  template: `<p>{{ userName }}</p>`
})
export class UserComponent {
  userName: string = '';
  
  // Dependency injected via constructor
  constructor(private userService: UserService) {
    this.userName = userService.getCurrentUser();
  }
}
```

### Providing Dependencies

#### Method 1: providedIn Root (Recommended)
```typescript
@Injectable({
  providedIn: 'root'
})
export class MyService { }
```

#### Method 2: providers Array in Component
```typescript
@Component({
  selector: 'app-my',
  providers: [MyService],  // Scoped to this component
  template: ''
})
export class MyComponent { }
```

#### Method 3: Bootstrap Application (Functional API)
```typescript
// main.ts
bootstrapApplication(AppComponent, {
  providers: [
    MyService,
    { provide: HttpClient, useClass: CustomHttpClient }
  ]
});
```

#### Method 4: Feature Module (Pre-Angular 14)
```typescript
@NgModule({
  providers: [MyService]
})
export class MyFeatureModule { }
```

### Dependency Resolution Strategies

```typescript
// 1. Class provider (most common)
{ provide: PaymentService, useClass: StripePaymentService }

// 2. Factory provider
{ 
  provide: AuthService, 
  useFactory: authServiceFactory,
  deps: [HttpClient, Config]
}

// 3. Value provider
{ provide: ApiBaseUrl, useValue: 'https://api.example.com' }

// 4. Alias provider
{ provide: OldServiceName, useExisting: NewServiceName }
```

### Injecting Dependencies in Services

```typescript
@Injectable()
export class NotificationService {
  constructor(
    private http: HttpClient,
    private router: Router,
    @Inject(ApiBaseUrl) private apiUrl: string
  ) {}
}
```

### Optional Dependencies

```typescript
import { Optional } from '@angular/core';

@Component({
  selector: 'app-my',
  standalone: true
})
export class MyComponent {
  constructor(@Optional() private logger: LoggerService) {
    // logger could be null
    if (this.logger) {
      this.logger.log('Service available');
    }
  }
}
```

---

## Directives

### What are Directives?

Directives are classes that add behavior to elements in the DOM. There are three types:

1. **Component Directives** - Directives with a template
2. **Structural Directives** - Change the structure of the DOM (add/remove elements)
3. **Attribute Directives** - Change appearance or behavior of an element

### Structural Directives

#### *ngIf - Conditional Rendering
```typescript
<div *ngIf="condition; else elseBlock">
  This shows when condition is true
</div>

<ng-template #elseBlock>
  This shows when condition is false
</ng-template>

// Or using new control flow syntax (Angular 17+)
@if (condition) {
  <div>Content</div>
} @else {
  <div>Else content</div>
}
```

#### *ngFor - List Rendering
```typescript
<ul>
  <li *ngFor="let item of items; let i = index; let isEven = even">
    {{ i }}: {{ item }} (Even: {{ isEven }})
  </li>
</ul>

// New syntax (Angular 17+)
@for (item of items; track item.id) {
  <li>{{ item.name }}</li>
}
```

#### *ngSwitch - Switch/Case Rendering
```typescript
<div [ngSwitch]="status">
  <p *ngSwitchCase="'active'">Active Status</p>
  <p *ngSwitchCase="'inactive'">Inactive Status</p>
  <p *ngSwitchDefault>Unknown Status</p>
</div>

// New syntax (Angular 17+)
@switch (status) {
  @case ('active') {
    <p>Active</p>
  }
  @case ('inactive') {
    <p>Inactive</p>
  }
  @default {
    <p>Unknown</p>
  }
}
```

### Attribute Directives

#### Built-in Attribute Directives

```typescript
// ngClass - Dynamic CSS classes
<div [ngClass]="{ 'active': isActive, 'disabled': isDisabled }">
  Content
</div>

// ngStyle - Dynamic inline styles
<div [ngStyle]="{ 'color': textColor, 'font-size': fontSize + 'px' }">
  Styled content
</div>

// ngModel - Two-way binding (requires FormsModule)
<input [(ngModel)]="userName" />

// Form attributes
<input [disabled]="isDisabled" [readonly]="isReadonly" />
```

### Custom Attribute Directives

```typescript
import { Directive, ElementRef, HostListener, Input } from '@angular/core';

@Directive({
  selector: '[appHighlight]',
  standalone: true
})
export class HighlightDirective {
  @Input() highlightColor = 'yellow';
  
  constructor(private el: ElementRef) {}
  
  @HostListener('mouseenter') onMouseEnter() {
    this.setHighlight(this.highlightColor);
  }
  
  @HostListener('mouseleave') onMouseLeave() {
    this.setHighlight('transparent');
  }
  
  private setHighlight(color: string) {
    this.el.nativeElement.style.backgroundColor = color;
  }
}

// Usage
<p appHighlight highlightColor="blue">Hover me</p>
```

### Custom Structural Directives

```typescript
import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';

@Directive({
  selector: '[appUnless]',
  standalone: true
})
export class UnlessDirective {
  @Input()
  set appUnless(condition: boolean) {
    if (!condition) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainer.clear();
    }
  }
  
  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef
  ) {}
}

// Usage
<p *appUnless="condition">Show when condition is false</p>
```

---

## Components & Lifecycle

### Component Basics

```typescript
@Component({
  selector: 'app-my-component',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h1>{{ title }}</h1>
    <button (click)="incrementCount()">Count: {{ count }}</button>
  `,
  styles: [`
    h1 { color: blue; }
  `]
})
export class MyComponent {
  title = 'My Component';
  count = 0;
  
  incrementCount() {
    this.count++;
  }
}
```

### Lifecycle Hooks

Angular calls lifecycle hooks at specific times during component creation, update, and destruction:

| Hook | Purpose | Usage |
|------|---------|-------|
| `ngOnInit()` | Initialize component after inputs set | Load initial data |
| `ngOnChanges(changes)` | Called before/after input properties change | React to input changes |
| `ngDoCheck()` | Custom change detection logic | Complex change detection |
| `ngAfterContentInit()` | Content projected into component initialized | Access ng-content |
| `ngAfterContentChecked()` | Projected content checked for changes | Monitor projected changes |
| `ngAfterViewInit()` | Component views initialized | Access @ViewChild |
| `ngAfterViewChecked()` | Component views checked | Monitor view changes |
| `ngOnDestroy()` | Component about to be destroyed | Cleanup, unsubscribe |

```typescript
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-lifecycle',
  standalone: true,
  template: '<p>{{ message }}</p>'
})
export class LifecycleComponent implements OnInit, OnDestroy {
  message = '';
  private subscription: Subscription;
  
  ngOnInit() {
    console.log('Component initialized');
    this.message = 'Initialized';
  }
  
  ngOnDestroy() {
    console.log('Component destroyed');
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
```

### Input & Output

```typescript
// Parent-to-child: @Input
@Component({
  selector: 'app-child',
  standalone: true
})
export class ChildComponent {
  @Input() userName: string = '';
  @Input() userAge: number = 0;
}

// Usage
<app-child [userName]="'John'" [userAge]="25"></app-child>

// Child-to-parent: @Output
@Component({
  selector: 'app-child-emitter',
  standalone: true
})
export class ChildEmitterComponent {
  @Output() userSelected = new EventEmitter<User>();
  
  selectUser(user: User) {
    this.userSelected.emit(user);
  }
}

// Usage
<app-child-emitter (userSelected)="onUserSelect($event)"></app-child-emitter>
```

### Change Detection

```typescript
import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-performance',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: '<p>{{ data }}</p>'
})
export class PerformanceComponent {
  data = 'Initial data';
  
  constructor(private cdr: ChangeDetectorRef) {}
  
  updateData() {
    this.data = 'Updated data';
    this.cdr.markForCheck(); // Manually trigger change detection
  }
}
```

---

## Reactive Programming & RxJS

### What is RxJS?

RxJS (Reactive Extensions for JavaScript) is a library for composing asynchronous and event-based programs using observable sequences.

### Key Concepts

#### Observables
An Observable is a lazy collection of multiple values over time.

```typescript
import { Observable } from 'rxjs';

// Creating an Observable
const myObservable = new Observable(subscriber => {
  subscriber.next(1);
  subscriber.next(2);
  subscriber.next(3);
  subscriber.complete();
});

// Subscribing to an Observable
myObservable.subscribe(value => console.log(value));
```

#### Subjects
A Subject is both an Observable and an Observer.

```typescript
import { Subject } from 'rxjs';

const subject = new Subject<number>();

subject.subscribe(value => console.log('Observer 1:', value));
subject.subscribe(value => console.log('Observer 2:', value));

subject.next(1); // Both observers receive 1
subject.next(2); // Both observers receive 2
```

#### Common Operators

```typescript
import { 
  map, filter, switchMap, mergeMap, 
  debounceTime, distinctUntilChanged,
  take, takeUntil, catchError
} from 'rxjs/operators';

// map - Transform values
data$.pipe(
  map(item => ({ ...item, name: item.name.toUpperCase() }))
);

// filter - Filter values
data$.pipe(
  filter(item => item.age > 18)
);

// switchMap - Switch to new observable (cancels previous)
searchTerm$.pipe(
  switchMap(term => this.searchService.search(term))
);

// debounceTime - Wait for pause before emitting
searchInput$.pipe(
  debounceTime(300),
  distinctUntilChanged(),
  switchMap(term => this.searchService.search(term))
);

// take - Emit only first N values
data$.pipe(take(5));

// takeUntil - Emit until another observable emits
data$.pipe(
  takeUntil(destroy$)
);

// catchError - Handle errors
data$.pipe(
  catchError(error => {
    console.error(error);
    return of([]); // Return fallback value
  })
);
```

### Async Pipe

The async pipe subscribes to an Observable and automatically unsubscribes on component destroy.

```typescript
@Component({
  selector: 'app-async-demo',
  standalone: true,
  imports: [CommonModule],
  template: `
    <p>{{ user$ | async }}</p>
    <ul>
      <li *ngFor="let item of items$ | async">
        {{ item.name }}
      </li>
    </ul>
  `
})
export class AsyncDemoComponent {
  user$ = this.userService.getUser();
  items$ = this.itemService.getItems();
  
  constructor(
    private userService: UserService,
    private itemService: ItemService
  ) {}
}
```

### Custom Services with RxJS

```typescript
@Injectable({
  providedIn: 'root'
})
export class StateService {
  private stateSubject = new BehaviorSubject<AppState>({
    loading: false,
    data: []
  });
  
  state$ = this.stateSubject.asObservable();
  
  updateState(newState: Partial<AppState>) {
    const current = this.stateSubject.getValue();
    this.stateSubject.next({ ...current, ...newState });
  }
  
  getData() {
    this.updateState({ loading: true });
    
    this.http.get('/api/data').pipe(
      tap(data => this.updateState({ data, loading: false })),
      catchError(error => {
        this.updateState({ loading: false });
        return throwError(() => error);
      })
    ).subscribe();
  }
}
```

### Angular Signals (Angular 14+)

Signals are a new reactive primitive that provides fine-grained reactivity.

```typescript
import { signal, computed, effect } from '@angular/core';

@Component({
  selector: 'app-signals',
  standalone: true,
  template: `
    <p>Count: {{ count() }}</p>
    <p>Double: {{ doubleCount() }}</p>
    <button (click)="incrementCount()">Increment</button>
  `
})
export class SignalsComponent {
  count = signal(0);
  doubleCount = computed(() => this.count() * 2);
  
  constructor() {
    effect(() => {
      console.log(`Count changed to ${this.count()}`);
    });
  }
  
  incrementCount() {
    this.count.set(this.count() + 1);
    // or: this.count.update(val => val + 1);
  }
}
```

---

## .NET Microservices Architecture

### What are Microservices?

Microservices is an architectural style that structures an application as a collection of loosely coupled, independently deployable services that communicate over well-defined APIs.

### Microservices Characteristics

- **Independent Deployment:** Each service can be deployed independently
- **Loose Coupling:** Services communicate through APIs, not shared databases
- **Bounded Contexts:** Each service owns its data and business logic
- **Technology Diversity:** Different services can use different tech stacks
- **Scalability:** Services can scale independently based on demand
- **Resilience:** Failure in one service doesn't crash the entire system

### Microservices in CapFinLoan Architecture

```
CapFinLoan.Backend/
├── AdminService/          # Admin operations microservice
│   ├── CapFinLoan.Admin.API/
│   ├── CapFinLoan.Admin.Application/
│   ├── CapFinLoan.Admin.Domain/
│   ├── CapFinLoan.Admin.Infrastructure/
│   └── CapFinLoan.Admin.Persistence/
├── ApplicationService/    # Loan application processing
│   ├── CapFinLoan.Application.API/
│   ├── CapFinLoan.Application.Application/
│   ├── CapFinLoan.Application.Domain/
│   ├── CapFinLoan.Application.Infrastructure/
│   └── CapFinLoan.Application.Persistence/
├── AuthService/          # Authentication & authorization
│   └── [Similar structure]
├── DocumentService/      # Document management
│   └── [Similar structure]
└── ApiGateway/          # Single entry point for frontend
    └── CapFinLoan.Gateway.API/
```

### Service Communication Patterns

#### Synchronous: REST/HTTP
```csharp
// Service A calling Service B via HTTP
public class ApplicationService
{
    private readonly HttpClient _httpClient;
    
    public async Task<AdminReviewDto> GetReviewAsync(int applicationId)
    {
        var response = await _httpClient.GetAsync(
            $"https://admin-service/api/reviews/{applicationId}"
        );
        return await response.Content.ReadAsAsync<AdminReviewDto>();
    }
}
```

#### Asynchronous: Message Queue (RabbitMQ)
```csharp
// Service A publishing event
public async Task PublishApplicationSubmittedAsync(int applicationId)
{
    var @event = new ApplicationSubmittedEvent { ApplicationId = applicationId };
    await _messagePublisher.PublishAsync(@event);
}

// Service B listening for event
public void OnApplicationSubmitted(ApplicationSubmittedEvent @event)
{
    // Process the event
    _logger.LogInformation($"Application {event.ApplicationId} submitted");
}
```

### Challenges in Microservices

| Challenge | Solution |
|-----------|----------|
| **Distributed Transactions** | Saga Pattern, Event Sourcing |
| **Network Latency** | Caching, Service Mesh |
| **Data Consistency** | Eventual Consistency, Event-driven |
| **Service Discovery** | Consul, Kubernetes, Azure Service Fabric |
| **Monitoring & Logging** | Centralized logging, APM tools |
| **API Versioning** | API Gateway, Contract Testing |

---

## 3-Layer Pattern

### What is 3-Layer Architecture?

The 3-Layer (or N-Layer) architecture divides the application into three main layers:

1. **Presentation Layer (API/UI)** - Handles HTTP requests and responses
2. **Application/Business Logic Layer** - Contains business rules and use cases
3. **Data Access Layer (Persistence)** - Handles database operations

### Benefits

- **Separation of Concerns:** Each layer has a specific responsibility
- **Maintainability:** Changes in one layer don't affect others
- **Testability:** Each layer can be tested independently
- **Reusability:** Business logic can be used by different clients

### 3-Layer Implementation in CapFinLoan

#### Layer 1: API/Presentation Layer
```csharp
// CapFinLoan.Admin.API/Controllers/ApplicationsController.cs
[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly ILoanApplicationService _service;
    
    public ApplicationsController(ILoanApplicationService service)
    {
        _service = service;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetApplication(int id)
    {
        var application = await _service.GetApplicationAsync(id);
        return Ok(application);
    }
    
    [HttpPost("{id}/start-review")]
    public async Task<IActionResult> StartReview(int id)
    {
        await _service.StartReviewAsync(id);
        return NoContent();
    }
}
```

**Responsibilities:**
- Receive HTTP requests
- Validate input parameters
- Call service methods
- Return HTTP responses
- Handle authentication/authorization

#### Layer 2: Application/Business Logic Layer
```csharp
// CapFinLoan.Admin.Application/Services/LoanApplicationService.cs
public interface ILoanApplicationService
{
    Task<LoanApplicationDto> GetApplicationAsync(int id);
    Task StartReviewAsync(int applicationId);
    Task ApproveApplicationAsync(int applicationId, decimal sanctionAmount);
    Task RejectApplicationAsync(int applicationId, string reason);
}

public class LoanApplicationService : ILoanApplicationService
{
    private readonly ILoanApplicationRepository _repository;
    private readonly IDocumentService _documentService;
    private readonly IMessagePublisher _messagePublisher;
    
    public async Task<LoanApplicationDto> GetApplicationAsync(int id)
    {
        // Fetch from repository
        var application = await _repository.GetByIdAsync(id);
        if (application == null)
            throw new ApplicationNotFoundException($"Application {id} not found");
        
        // Fetch related documents
        var documents = await _documentService.GetDocumentsAsync(id);
        
        // Map to DTO
        return MapToDto(application, documents);
    }
    
    public async Task StartReviewAsync(int applicationId)
    {
        var application = await _repository.GetByIdAsync(applicationId);
        
        if (application.Status != ApplicationStatus.Submitted)
            throw new InvalidOperationException("Application must be in Submitted status");
        
        application.Status = ApplicationStatus.UnderReview;
        application.ReviewStartedDate = DateTime.UtcNow;
        
        await _repository.UpdateAsync(application);
        await _repository.SaveChangesAsync();
        
        // Publish event for other services
        await _messagePublisher.PublishAsync(
            new ReviewStartedEvent { ApplicationId = applicationId }
        );
    }
    
    public async Task ApproveApplicationAsync(int applicationId, decimal sanctionAmount)
    {
        var application = await _repository.GetByIdAsync(applicationId);
        
        if (application.Status != ApplicationStatus.UnderReview)
            throw new InvalidOperationException("Can only approve Under Review applications");
        
        application.Status = ApplicationStatus.Approved;
        application.SanctionAmount = sanctionAmount;
        application.ApprovedDate = DateTime.UtcNow;
        
        await _repository.UpdateAsync(application);
        await _repository.SaveChangesAsync();
        
        // Publish event
        await _messagePublisher.PublishAsync(
            new ApplicationApprovedEvent 
            { 
                ApplicationId = applicationId,
                SanctionAmount = sanctionAmount
            }
        );
    }
}
```

**Responsibilities:**
- Implement business logic and rules
- Coordinate between repositories and external services
- Perform validations
- Publish domain events
- Handle errors and exceptions

#### Layer 3: Data Access/Persistence Layer
```csharp
// CapFinLoan.Admin.Domain/Entities/LoanApplication.cs
public class LoanApplication
{
    public int Id { get; set; }
    public int ApplicantUserId { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ReviewStartedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public decimal? SanctionAmount { get; set; }
    public decimal InterestRate { get; set; }
    public string AdminRemarks { get; set; }
}

public enum ApplicationStatus
{
    Submitted,
    UnderReview,
    Approved,
    Rejected
}

// CapFinLoan.Admin.Persistence/Repositories/LoanApplicationRepository.cs
public interface ILoanApplicationRepository
{
    Task<LoanApplication> GetByIdAsync(int id);
    Task<IEnumerable<LoanApplication>> GetAllAsync();
    Task<IEnumerable<LoanApplication>> GetByStatusAsync(ApplicationStatus status);
    Task AddAsync(LoanApplication application);
    Task UpdateAsync(LoanApplication application);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
}

public class LoanApplicationRepository : ILoanApplicationRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public LoanApplicationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<LoanApplication> GetByIdAsync(int id)
    {
        return await _dbContext.LoanApplications.FindAsync(id);
    }
    
    public async Task<IEnumerable<LoanApplication>> GetAllAsync()
    {
        return await _dbContext.LoanApplications.ToListAsync();
    }
    
    public async Task UpdateAsync(LoanApplication application)
    {
        _dbContext.LoanApplications.Update(application);
    }
    
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

// CapFinLoan.Admin.Persistence/Data/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    public DbSet<LoanApplication> LoanApplications { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoanApplication>()
            .HasKey(x => x.Id);
        
        modelBuilder.Entity<LoanApplication>()
            .Property(x => x.Status)
            .HasConversion(new EnumToStringConverter<ApplicationStatus>());
    }
}
```

**Responsibilities:**
- Database mapping through EF Core
- CRUD operations via repositories
- Query optimization
- Transaction management

### Dependency Flow in 3-Layer

```
Controller (API Layer)
    ↓ depends on
Service (Business Logic Layer)
    ↓ depends on
Repository (Data Access Layer)
    ↓ accesses
Database
```

---

## Web API in .NET

### What is a Web API?

A Web API is a web-based application programming interface that enables communication between applications using HTTP protocol. In .NET, we use ASP.NET Core to build RESTful Web APIs.

### REST Principles

- **Client-Server:** Separation between client and server
- **Statelessness:** Each request contains all information needed
- **Resource-Based:** APIs organized around resources (nouns) not actions (verbs)
- **Standard Methods:** Use HTTP verbs (GET, POST, PUT, DELETE, PATCH)
- **Representation:** Resources represented in JSON/XML

### REST Endpoints

```
GET    /api/applications           - Get all applications
GET    /api/applications/5         - Get application with ID 5
POST   /api/applications           - Create new application
PUT    /api/applications/5         - Update application 5
PATCH  /api/applications/5         - Partial update of application 5
DELETE /api/applications/5         - Delete application 5
```

### ASP.NET Core Web API Structure

#### Program.cs - Dependency Injection & Middleware Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application services
builder.Services.AddScoped<ILoanApplicationService, LoanApplicationService>();
builder.Services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
    );
});

// Add HttpClient for inter-service communication
builder.Services.AddHttpClient<AdminServiceClient>();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

#### Controller Implementation

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class ApplicationsController : ControllerBase
{
    private readonly ILoanApplicationService _loanService;
    private readonly ILogger<ApplicationsController> _logger;
    
    public ApplicationsController(
        ILoanApplicationService loanService,
        ILogger<ApplicationsController> logger
    )
    {
        _loanService = loanService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all loan applications
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LoanApplicationDto>>> GetAll()
    {
        try
        {
            var applications = await _loanService.GetAllApplicationsAsync();
            return Ok(applications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applications");
            return StatusCode(500, "Internal server error");
        }
    }
    
    /// <summary>
    /// Get application by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanApplicationDto>> GetById(int id)
    {
        var application = await _loanService.GetApplicationAsync(id);
        if (application == null)
            return NotFound(new { message = $"Application {id} not found" });
        
        return Ok(application);
    }
    
    /// <summary>
    /// Create new loan application
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanApplicationDto>> Create(
        [FromBody] CreateLoanApplicationRequest request
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var createdApplication = await _loanService.CreateApplicationAsync(request);
        return CreatedAtAction(nameof(GetById), 
            new { id = createdApplication.Id }, 
            createdApplication
        );
    }
    
    /// <summary>
    /// Update loan application
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateLoanApplicationRequest request
    )
    {
        var result = await _loanService.UpdateApplicationAsync(id, request);
        if (!result)
            return NotFound();
        
        return NoContent();
    }
    
    /// <summary>
    /// Approve loan application
    /// </summary>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Approve(
        int id,
        [FromBody] ApproveApplicationRequest request
    )
    {
        await _loanService.ApproveApplicationAsync(id, request.SanctionAmount);
        return NoContent();
    }
    
    /// <summary>
    /// Delete loan application
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        await _loanService.DeleteApplicationAsync(id);
        return NoContent();
    }
}
```

#### API Models (DTOs)

```csharp
// Request DTOs
public class CreateLoanApplicationRequest
{
    [Required]
    public int LoanAmount { get; set; }
    
    [Required]
    [StringLength(100)]
    public string LoanPurpose { get; set; }
    
    public string Note { get; set; }
}

public class UpdateLoanApplicationRequest
{
    public string AdminRemarks { get; set; }
    public decimal InterestRate { get; set; }
}

public class ApproveApplicationRequest
{
    [Required]
    public decimal SanctionAmount { get; set; }
}

// Response DTO
public class LoanApplicationDto
{
    public int Id { get; set; }
    public int ApplicantUserId { get; set; }
    public int LoanAmount { get; set; }
    public string LoanPurpose { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public decimal? SanctionAmount { get; set; }
    public decimal InterestRate { get; set; }
}
```

### Error Handling in Web API

```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse();
        
        switch (exception)
        {
            case ApplicationNotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = exception.Message;
                break;
                
            case InvalidOperationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = exception.Message;
                break;
                
            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = "Internal server error";
                break;
        }
        
        return context.Response.WriteAsJsonAsync(response);
    }
}

public class ErrorResponse
{
    public string Message { get; set; }
    public string TraceId { get; set; }
}
```

### API Versioning

```csharp
// v1
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ApplicationsV1Controller : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() { }
}

// v2 with different response
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
public class ApplicationsV2Controller : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() { }
}
```

---

## Message Queuing with RabbitMQ

### What is RabbitMQ?

RabbitMQ is an open-source message broker that implements the Advanced Message Queuing Protocol (AMQP). It enables reliable, asynchronous communication between microservices.

### Key Concepts

#### Producer
A producer sends messages to the message broker.

#### Consumer
A consumer receives and processes messages from the queue.

#### Queue
A buffer that stores messages until they are consumed.

#### Exchange
Routes messages to queues based on routing rules.

#### Binding
Connects exchanges to queues with specific routing patterns.

### RabbitMQ Architecture

```
Producer → Exchange → Queue → Consumer
                ↓
           Routing Rules
```

### RabbitMQ in .NET - Implementation

#### 1. Setup RabbitMQ Connection

```csharp
// CapFinLoan.Application.Infrastructure/MessageQueue/RabbitMqConnection.cs
public interface IRabbitMqConnection
{
    IConnection CreateConnection();
}

public class RabbitMqConnection : IRabbitMqConnection
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqConnection> _logger;
    
    public RabbitMqConnection(IConfiguration configuration, ILogger<RabbitMqConnection> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public IConnection CreateConnection()
    {
        var connectionString = _configuration.GetConnectionString("RabbitMq");
        var factory = new ConnectionFactory() { Uri = new Uri(connectionString) };
        
        try
        {
            return factory.CreateConnection();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ connection failed");
            throw;
        }
    }
}
```

#### 2. Define Events/Messages

```csharp
// CapFinLoan.Application.Domain/Events/ApplicationSubmittedEvent.cs
public class ApplicationSubmittedEvent
{
    public int ApplicationId { get; set; }
    public int ApplicantUserId { get; set; }
    public DateTime SubmittedDate { get; set; }
    public decimal LoanAmount { get; set; }
}

public class ReviewStartedEvent
{
    public int ApplicationId { get; set; }
    public DateTime ReviewStartedDate { get; set; }
}

public class ApplicationApprovedEvent
{
    public int ApplicationId { get; set; }
    public decimal SanctionAmount { get; set; }
    public DateTime ApprovedDate { get; set; }
}
```

#### 3. Message Publisher

```csharp
// CapFinLoan.Application.Infrastructure/MessageQueue/IMessagePublisher.cs
public interface IMessagePublisher
{
    Task PublishAsync<T>(T @event) where T : class;
}

// CapFinLoan.Application.Infrastructure/MessageQueue/RabbitMqPublisher.cs
public class RabbitMqPublisher : IMessagePublisher
{
    private readonly IRabbitMqConnection _rabbitMqConnection;
    private readonly ILogger<RabbitMqPublisher> _logger;
    
    public RabbitMqPublisher(IRabbitMqConnection rabbitMqConnection, ILogger<RabbitMqPublisher> logger)
    {
        _rabbitMqConnection = rabbitMqConnection;
        _logger = logger;
    }
    
    public async Task PublishAsync<T>(T @event) where T : class
    {
        using (var connection = _rabbitMqConnection.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            var eventName = typeof(T).Name;
            
            // Declare exchange
            channel.ExchangeDeclare(
                exchange: "loan_application_exchange",
                type: ExchangeType.Topic,
                durable: true
            );
            
            // Serialize event
            var json = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(json);
            
            // Publish message
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Type = eventName;
            properties.ContentType = "application/json";
            
            channel.BasicPublish(
                exchange: "loan_application_exchange",
                routingKey: eventName,
                basicProperties: properties,
                body: body
            );
            
            _logger.LogInformation($"Published event: {eventName}");
            
            await Task.CompletedTask;
        }
    }
}
```

#### 4. Message Consumer

```csharp
// CapFinLoan.Admin.Infrastructure/MessageQueue/RabbitMqConsumer.cs
public interface IEventConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
}

public class RabbitMqEventConsumer : IEventConsumer, IDisposable
{
    private readonly IRabbitMqConnection _rabbitMqConnection;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqEventConsumer> _logger;
    private IConnection _connection;
    private IModel _channel;
    
    public RabbitMqEventConsumer(
        IRabbitMqConnection rabbitMqConnection,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqEventConsumer> logger
    )
    {
        _rabbitMqConnection = rabbitMqConnection;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _connection = _rabbitMqConnection.CreateConnection();
        _channel = _connection.CreateModel();
        
        // Declare exchange and queue
        _channel.ExchangeDeclare(
            exchange: "loan_application_exchange",
            type: ExchangeType.Topic,
            durable: true
        );
        
        var queueName = _channel.QueueDeclare(
            queue: "admin_service_queue",
            durable: true,
            exclusive: false,
            autoDelete: false
        ).QueueName;
        
        // Bind queue to exchange
        _channel.QueueBind(
            queue: queueName,
            exchange: "loan_application_exchange",
            routingKey: "ApplicationSubmittedEvent"
        );
        
        _channel.BasicQos(0, 1, false);
        
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                await HandleMessageAsync(ea);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };
        
        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer
        );
        
        _logger.LogInformation("RabbitMQ consumer started");
        
        await Task.CompletedTask;
    }
    
    private async Task HandleMessageAsync(BasicDeliverEventArgs ea)
    {
        var eventName = ea.BasicProperties.Type;
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
        
        _logger.LogInformation($"Processing event: {eventName}");
        
        using (var scope = _serviceProvider.CreateScope())
        {
            var handler = scope.ServiceProvider.GetService(
                Type.GetType($"CapFinLoan.Admin.Application.Services.{eventName}Handler")
            );
            
            if (handler != null)
            {
                var method = handler.GetType().GetMethod("HandleAsync");
                var @event = JsonSerializer.Deserialize(json, Type.GetType($"CapFinLoan.Domain.Events.{eventName}"));
                
                await (Task)method.Invoke(handler, new[] { @event });
            }
        }
    }
    
    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}

// Event handler
public class ApplicationSubmittedEventHandler
{
    private readonly ILoanApplicationService _loanService;
    
    public ApplicationSubmittedEventHandler(ILoanApplicationService loanService)
    {
        _loanService = loanService;
    }
    
    public async Task HandleAsync(ApplicationSubmittedEvent @event)
    {
        // Process the event
        await _loanService.ProcessApplicationSubmissionAsync(@event);
    }
}
```

#### 5. Register in Program.cs

```csharp
// Program.cs
builder.Services.AddScoped<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddScoped<IEventConsumer, RabbitMqEventConsumer>();

// Register event handlers
builder.Services.AddScoped<ApplicationSubmittedEventHandler>();

// Start consumer as hosted service
builder.Services.AddHostedService<RabbitMqConsumerHostedService>();

public class RabbitMqConsumerHostedService : BackgroundService
{
    private readonly IEventConsumer _consumer;
    
    public RabbitMqConsumerHostedService(IEventConsumer consumer)
    {
        _consumer = consumer;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.StartAsync(stoppingToken);
    }
}
```

#### 6. appsettings.json Configuration

```json
{
  "ConnectionStrings": {
    "RabbitMq": "amqp://guest:guest@localhost"
  },
  "RabbitMqOptions": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  }
}
```

---

## Saga Pattern

### What is the Saga Pattern?

The Saga Pattern is a pattern for managing long-running transactions in a microservices architecture. Instead of using distributed transactions (which are problematic in microservices), a saga breaks down a long transaction into a series of local transactions, each triggered by a service.

### Types of Sagas

#### 1. Choreography Saga
Services listen to events and respond by issuing events.

```
User Service
    ↓ (User Created Event)
Application Service
    ↓ (Application Created Event)
Document Service
    ↓ (Documents Prepared Event)
Admin Service
    ↓ (Review Started Event)
```

#### 2. Orchestration Saga
A central orchestrator tells services what to do.

```
Saga Orchestrator
├→ User Service (Create User)
├→ Application Service (Create Application)
├→ Document Service (Prepare Documents)
└→ Admin Service (Start Review)
```

### Loan Application Saga Example

#### Saga Definition

```csharp
// CapFinLoan.Application.Domain/Sagas/LoanApplicationSaga.cs
public enum LoanApplicationSagaStatus
{
    Started,
    UserDetailsValidated,
    DocumentsCollected,
    DocumentsVerified,
    ReviewCompleted,
    Completed,
    Failed
}

public class LoanApplicationSaga
{
    public string SagaId { get; set; }
    public int ApplicationId { get; set; }
    public int ApplicantUserId { get; set; }
    public LoanApplicationSagaStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public Dictionary<string, object> Data { get; set; }
}
```

#### Orchestration-based Saga Implementation

```csharp
// CapFinLoan.Application.Application/Services/LoanApplicationSagaOrchestrator.cs
public interface ILoanApplicationSagaOrchestrator
{
    Task<LoanApplicationSaga> StartSagaAsync(CreateLoanApplicationRequest request);
}

public class LoanApplicationSagaOrchestrator : ILoanApplicationSagaOrchestrator
{
    private readonly IAuthServiceClient _authService;
    private readonly IDocumentServiceClient _documentService;
    private readonly IAdminServiceClient _adminService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<LoanApplicationSagaOrchestrator> _logger;
    
    public async Task<LoanApplicationSaga> StartSagaAsync(CreateLoanApplicationRequest request)
    {
        var saga = new LoanApplicationSaga
        {
            SagaId = Guid.NewGuid().ToString(),
            Status = LoanApplicationSagaStatus.Started,
            CreatedDate = DateTime.UtcNow,
            Data = new Dictionary<string, object> { { "Request", request } }
        };
        
        try
        {
            // Step 1: Validate User Details
            _logger.LogInformation($"[Saga {saga.SagaId}] Step 1: Validating user details");
            var userValidated = await _authService.ValidateUserAsync(request.ApplicantUserId);
            if (!userValidated)
                throw new Exception("User validation failed");
            
            saga.Status = LoanApplicationSagaStatus.UserDetailsValidated;
            
            // Step 2: Create Application
            _logger.LogInformation($"[Saga {saga.SagaId}] Step 2: Creating application");
            var application = new LoanApplication
            {
                ApplicantUserId = request.ApplicantUserId,
                LoanAmount = request.LoanAmount,
                Status = ApplicationStatus.Submitted
            };
            saga.ApplicationId = application.Id;
            saga.Data["ApplicationId"] = application.Id;
            
            // Step 3: Collect Documents
            _logger.LogInformation($"[Saga {saga.SagaId}] Step 3: Collecting documents");
            var documentsCollected = await _documentService.CollectDocumentsAsync(
                application.Id,
                request.ApplicantUserId
            );
            
            saga.Status = LoanApplicationSagaStatus.DocumentsCollected;
            
            // Step 4: Verify Documents
            _logger.LogInformation($"[Saga {saga.SagaId}] Step 4: Verifying documents");
            var documentsVerified = await _documentService.VerifyDocumentsAsync(application.Id);
            
            saga.Status = LoanApplicationSagaStatus.DocumentsVerified;
            
            // Step 5: Initiate Review
            _logger.LogInformation($"[Saga {saga.SagaId}] Step 5: Initiating review");
            await _adminService.InitiateReviewAsync(application.Id);
            
            saga.Status = LoanApplicationSagaStatus.ReviewCompleted;
            saga.Status = LoanApplicationSagaStatus.Completed;
            
            // Publish completion event
            await _messagePublisher.PublishAsync(
                new LoanApplicationSagaCompletedEvent { SagaId = saga.SagaId }
            );
            
            _logger.LogInformation($"[Saga {saga.SagaId}] Completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[Saga {saga.SagaId}] Failed");
            saga.Status = LoanApplicationSagaStatus.Failed;
            
            // Execute compensating transactions (rollback)
            await CompensateAsync(saga);
            
            throw;
        }
        
        return saga;
    }
    
    private async Task CompensateAsync(LoanApplicationSaga saga)
    {
        _logger.LogInformation($"[Saga {saga.SagaId}] Executing compensating transactions");
        
        // Compensating transaction: Delete created application
        if (saga.Data.TryGetValue("ApplicationId", out var appId))
        {
            await _adminService.DeleteApplicationAsync((int)appId);
        }
        
        // Compensating transaction: Remove collected documents
        if (saga.Data.TryGetValue("DocumentIds", out var docIds))
        {
            await _documentService.DeleteDocumentsAsync((List<int>)docIds);
        }
    }
}
```

#### Choreography-based Saga Implementation

```csharp
// CapFinLoan.Application.Service/Services/ApplicationService.cs
public class ApplicationService : IApplicationService
{
    private readonly ILoanApplicationRepository _repository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<ApplicationService> _logger;
    
    public async Task<LoanApplication> CreateApplicationAsync(CreateLoanApplicationRequest request)
    {
        var application = new LoanApplication
        {
            ApplicantUserId = request.ApplicantUserId,
            LoanAmount = request.LoanAmount,
            Status = ApplicationStatus.Submitted,
            CreatedDate = DateTime.UtcNow
        };
        
        await _repository.AddAsync(application);
        await _repository.SaveChangesAsync();
        
        // Publish event - Document Service will listen and do its part
        await _messagePublisher.PublishAsync(
            new ApplicationCreatedEvent
            {
                ApplicationId = application.Id,
                ApplicantUserId = request.ApplicantUserId
            }
        );
        
        _logger.LogInformation($"Application {application.Id} created, event published");
        
        return application;
    }
}

// CapFinLoan.Document.Service/Services/DocumentService.cs
public class DocumentEventHandler
{
    private readonly IDocumentRepository _repository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<DocumentEventHandler> _logger;
    
    public async Task HandleApplicationCreatedEventAsync(ApplicationCreatedEvent @event)
    {
        _logger.LogInformation($"Handling ApplicationCreatedEvent for app {event.ApplicationId}");
        
        try
        {
            // Collect and verify documents
            var documents = await _repository.CollectDocumentsAsync(@event.ApplicantUserId);
            await _repository.SaveChangesAsync();
            
            // Publish next step event
            await _messagePublisher.PublishAsync(
                new DocumentsCollectedEvent
                {
                    ApplicationId = @event.ApplicationId,
                    DocumentCount = documents.Count
                }
            );
            
            _logger.LogInformation($"Documents collected for app {event.ApplicationId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to collect documents for app {event.ApplicationId}");
            
            // Publish failure event for compensation
            await _messagePublisher.PublishAsync(
                new DocumentCollectionFailedEvent
                {
                    ApplicationId = @event.ApplicationId,
                    Reason = ex.Message
                }
            );
        }
    }
}

// CapFinLoan.Admin.Service/Services/AdminReviewEventHandler.cs
public class AdminReviewEventHandler
{
    private readonly ILoanApplicationService _service;
    private readonly IMessagePublisher _messagePublisher;
    
    public async Task HandleDocumentsCollectedEventAsync(DocumentsCollectedEvent @event)
    {
        // Initiate admin review
        await _service.InitiateReviewAsync(@event.ApplicationId);
        
        // Publish event for next step
        await _messagePublisher.PublishAsync(
            new ReviewInitiatedEvent { ApplicationId = @event.ApplicationId }
        );
    }
}
```

### Saga Patterns Comparison

| Aspect | Choreography | Orchestration |
|--------|--------------|----------------|
| **Decision Logic** | Distributed | Centralized |
| **Complexity** | Simple to understand | Complex to understand |
| **Testability** | Hard to test | Easy to test |
| **Debugging** | Hard to debug | Easy to debug |
| **Scalability** | Good for independent flows | Good for coordinated flows |
| **When to Use** | Event-driven workflows | Complex business processes |

---

## Best Practices

### Angular Best Practices

#### 1. Use Standalone Components
```typescript
// ✅ Good - Modern approach
@Component({
  selector: 'app-user',
  standalone: true,
  imports: [CommonModule]
})
export class UserComponent {}

// ❌ Avoid - Legacy approach
@NgModule({
  declarations: [UserComponent]
})
export class UserModule {}
```

#### 2. Use Lazy Loading
```typescript
// app.routes.ts
export const routes: Routes = [
  {
    path: 'admin',
    loadComponent: () => import('./pages/admin/admin.component')
      .then(m => m.AdminComponent)
  },
  {
    path: 'applications',
    loadChildren: () => import('./features/applications/applications.routes')
      .then(m => m.APPLICATIONS_ROUTES)
  }
];
```

#### 3. Unsubscribe from Observables
```typescript
// ✅ Good - Using takeUntil
export class MyComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  ngOnInit() {
    this.dataService.getData()
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => {});
  }
  
  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}

// ✅ Also good - Using async pipe
<p>{{ data$ | async }}</p>
```

#### 4. Organize Code by Feature
```
src/app/
├── features/
│   ├── applications/
│   │   ├── components/
│   │   ├── services/
│   │   ├── guards/
│   │   └── applications.routes.ts
│   ├── admin/
│   └── profile/
├── shared/
│   ├── components/
│   ├── services/
│   └── interceptors/
```

#### 5. Use Strong Typing
```typescript
// ✅ Good
export interface User {
  id: number;
  name: string;
  email: string;
}

export class UserService {
  getUser(id: number): Observable<User> {
    return this.http.get<User>(`/api/users/${id}`);
  }
}

// ❌ Avoid
export class UserService {
  getUser(id: number): Observable<any> {
    return this.http.get(`/api/users/${id}`);
  }
}
```

### .NET Best Practices

#### 1. Use Dependency Injection Properly
```csharp
// ✅ Good
public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;
    
    public UserService(IUserRepository repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}

// ❌ Avoid
public class UserService
{
    private readonly UserRepository _repository = new UserRepository();
    
    public UserService()
    {
    }
}
```

#### 2. Implement Proper Error Handling
```csharp
// ✅ Good
[HttpGet("{id}")]
public async Task<ActionResult<UserDto>> GetUser(int id)
{
    try
    {
        var user = await _userService.GetUserAsync(id);
        if (user == null)
            return NotFound();
        
        return Ok(user);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting user");
        return StatusCode(500, "Internal server error");
    }
}
```

#### 3. Use Async/Await
```csharp
// ✅ Good
public async Task<List<User>> GetUsersAsync()
{
    return await _context.Users.ToListAsync();
}

// ❌ Avoid
public List<User> GetUsers()
{
    return _context.Users.ToList(); // Blocks thread
}
```

#### 4. Use Repository Pattern
```csharp
// ✅ Good
public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task SaveChangesAsync();
}

public class UserService
{
    private readonly IUserRepository _repository;
    
    public async Task CreateUserAsync(CreateUserRequest request)
    {
        var user = new User { Name = request.Name };
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();
    }
}
```

#### 5. Validate Input Data
```csharp
// ✅ Good
[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    var user = await _userService.CreateUserAsync(request);
    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
}

public class CreateUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
```

### Microservices Best Practices

#### 1. Implement Circuit Breaker Pattern
```csharp
public class AdminServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<HttpResponseMessage> _policy;
    
    public AdminServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        // Circuit breaker: break after 3 failures, retry with exponential backoff
        _policy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync<HttpResponseMessage>(
                failureThreshold: 3,
                durationOfBreak: TimeSpan.FromSeconds(30)
            )
            .WrapAsync(
                Policy
                    .Handle<HttpRequestException>()
                    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .WaitAndRetryAsync<HttpResponseMessage>(
                        retryCount: 3,
                        sleepDurationProvider: attempt =>
                            TimeSpan.FromSeconds(Math.Pow(2, attempt))
                    )
            );
    }
    
    public async Task<AdminReviewDto> GetReviewAsync(int applicationId)
    {
        var response = await _policy.ExecuteAsync(() =>
            _httpClient.GetAsync($"https://admin-service/api/reviews/{applicationId}")
        );
        
        return await response.Content.ReadAsAsync<AdminReviewDto>();
    }
}
```

#### 2. Log Structured Data
```csharp
// ✅ Good - Structured logging
_logger.LogInformation("Application {ApplicationId} created by user {UserId}", 
    applicationId, userId);

// Usage with Serilog
Serilog.Log.Information(
    "Application {@Application} created", 
    application
);
```

#### 3. Implement Health Checks
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddCheck<RabbitMqHealthCheck>("rabbitmq");

app.MapHealthChecks("/health");
```

#### 4. Use API Gateway for Cross-Cutting Concerns
```csharp
// CapFinLoan.Gateway.API/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();
app.Run();

// appsettings.json
{
  "ReverseProxy": {
    "Routes": {
      "applications": {
        "ClusterId": "applications_cluster",
        "Match": { "Path": "/api/applications{**catch-all}" }
      },
      "admin": {
        "ClusterId": "admin_cluster",
        "Match": { "Path": "/api/admin{**catch-all}" }
      }
    },
    "Clusters": {
      "applications_cluster": {
        "Destinations": {
          "destination1": { "Address": "http://localhost:5001" }
        }
      },
      "admin_cluster": {
        "Destinations": {
          "destination1": { "Address": "http://localhost:5002" }
        }
      }
    }
  }
}
```

---

## Summary Table

| Topic | Key Concept | Key Benefit |
|-------|-------------|------------|
| **Directives** | Add behavior to DOM elements | Code reuse, cleaner templates |
| **DI** | Inject dependencies externally | Testability, loose coupling |
| **Components** | Building blocks of Angular apps | Modularity, reusability |
| **RxJS** | Reactive programming library | Async handling, event streams |
| **Signals** | Fine-grained reactivity | Better performance, simpler code |
| **3-Layer** | Separation of concerns | Maintainability, testability |
| **Web API** | HTTP-based service interface | Interoperability, standardization |
| **RabbitMQ** | Message broker | Asynchronous communication |
| **Saga** | Distributed transaction pattern | Consistency across services |
| **Microservices** | Service-oriented architecture | Scalability, independence |

---