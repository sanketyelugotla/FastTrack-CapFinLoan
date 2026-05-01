import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PortalTopbarComponent } from '../../shared/components/portal-topbar/portal-topbar.component';
import { SidebarComponent } from '../../shared/components/sidebar/sidebar.component';
import { ChatbotWidgetComponent } from '../../shared/components/chatbot/chatbot-widget.component';

@Component({
  selector: 'app-applicant-layout',
  imports: [RouterOutlet, PortalTopbarComponent, SidebarComponent, ChatbotWidgetComponent],
  templateUrl: './applicant-layout.component.html',
  styleUrl: './applicant-layout.component.css'
})
export class ApplicantLayoutComponent { }
