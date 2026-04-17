import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PortalTopbarComponent } from '../../shared/components/portal-topbar/portal-topbar.component';
import { SidebarComponent } from '../../shared/components/sidebar/sidebar.component';
import { ChatWidgetComponent } from '../../shared/components/chat-widget/chat-widget.component';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, PortalTopbarComponent, SidebarComponent, ChatWidgetComponent],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.css'
})
export class AdminLayoutComponent { }
