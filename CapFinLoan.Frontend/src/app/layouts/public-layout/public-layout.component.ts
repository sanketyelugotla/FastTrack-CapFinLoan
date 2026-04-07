import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { trigger, transition, style, animate, query, group } from '@angular/animations';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../shared/components/footer/footer.component';

@Component({
  selector: 'app-public-layout',
  imports: [RouterOutlet, NavbarComponent, FooterComponent],
  templateUrl: './public-layout.component.html',
  styleUrl: './public-layout.component.css',
  animations: [
    trigger('routeAnimations', [
      transition('LoginPage <=> SignupPage', [
        style({ position: 'relative' }),
        query(':enter, :leave', [
          style({
            position: 'absolute',
            width: '100%',
            top: 0,
            left: 0,
          })
        ], { optional: true }),
        group([
          query(':leave', [
            animate('400ms cubic-bezier(.35,0,.25,1)', style({ transform: 'translateX(100%)', opacity: 0 }))
          ], { optional: true }),
          query(':enter', [
            style({ transform: 'translateX(-100%)', opacity: 0 }),
            animate('400ms cubic-bezier(.35,0,.25,1)', style({ transform: 'translateX(0)', opacity: 1 }))
          ], { optional: true })
        ])
      ])
    ])
  ]
})
export class PublicLayoutComponent {
  prepareRoute(outlet: RouterOutlet) {
    return outlet && outlet.activatedRouteData && outlet.activatedRouteData['animation'];
  }
}
