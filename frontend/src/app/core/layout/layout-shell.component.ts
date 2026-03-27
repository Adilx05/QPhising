import { Component } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-layout-shell',
  templateUrl: './layout-shell.component.html',
  styleUrl: './layout-shell.component.css'
})
export class LayoutShellComponent {
  protected readonly navItems = [
    { label: 'Dashboard', route: '/dashboard' },
    { label: 'Campaigns', route: '/campaigns' },
    { label: 'Templates', route: '/templates' },
    { label: 'Tracking', route: '/tracking' },
    { label: 'Tasks', route: '/tasks' },
    { label: 'Analytics', route: '/analytics' },
    { label: 'Exports', route: '/exports' }
  ];
}
