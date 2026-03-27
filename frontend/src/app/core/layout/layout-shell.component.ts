import { Component, computed, inject } from '@angular/core';

import { AppRole, AppStateStore } from '../state/app-state.store';

interface NavigationItem {
  label: string;
  route: string;
  allowedRoles: readonly AppRole[];
}

@Component({
  standalone: false,
  selector: 'app-layout-shell',
  templateUrl: './layout-shell.component.html',
  styleUrl: './layout-shell.component.css'
})
export class LayoutShellComponent {
  private readonly appStateStore = inject(AppStateStore);

  private readonly navigationItems: readonly NavigationItem[] = [
    { label: 'Dashboard', route: '/dashboard', allowedRoles: ['Viewer', 'Operator', 'Admin'] },
    { label: 'Campaigns', route: '/campaigns', allowedRoles: ['Viewer', 'Operator', 'Admin'] },
    { label: 'Templates', route: '/templates', allowedRoles: ['Viewer', 'Operator', 'Admin'] },
    { label: 'Tracking', route: '/tracking', allowedRoles: ['Viewer', 'Operator', 'Admin'] },
    { label: 'Tasks', route: '/tasks', allowedRoles: ['Operator', 'Admin'] },
    { label: 'Analytics', route: '/analytics', allowedRoles: ['Viewer', 'Operator', 'Admin'] },
    { label: 'Exports', route: '/exports', allowedRoles: ['Viewer', 'Operator', 'Admin'] }
  ];

  protected readonly session = this.appStateStore.session;
  protected readonly navItems = computed(() =>
    this.navigationItems.filter((item) => this.appStateStore.canAccessAnyRole(item.allowedRoles))
  );
}
