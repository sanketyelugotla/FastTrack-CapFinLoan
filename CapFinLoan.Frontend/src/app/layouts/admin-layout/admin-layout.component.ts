import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../../shared/components/sidebar/sidebar.component';
import { ChatbotWidgetComponent } from '../../shared/components/chatbot/chatbot-widget.component';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, SidebarComponent, ChatbotWidgetComponent],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.css'
})
export class AdminLayoutComponent { }
