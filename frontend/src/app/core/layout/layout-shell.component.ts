import { Component, computed, inject } from '@angular/core';

import { AppStateStore } from '../state/app-state.store';

@Component({
  standalone: false,
  selector: 'app-layout-shell',
  templateUrl: './layout-shell.component.html',
  styleUrl: './layout-shell.component.css'
})
export class LayoutShellComponent {
  private readonly appStateStore = inject(AppStateStore);

  protected readonly session = this.appStateStore.session;
  protected readonly navItems = computed(() => {
    const role = this.appStateStore.currentRole();

    const baseItems = [
      { label: 'Dashboard', route: '/dashboard' },
      { label: 'Campaigns', route: '/campaigns' },
      { label: 'Templates', route: '/templates' },
      { label: 'Tracking', route: '/tracking' },
      { label: 'Tasks', route: '/tasks' },
      { label: 'Analytics', route: '/analytics' },
      { label: 'Exports', route: '/exports' }
    ];

    if (role === 'Viewer') {
      return baseItems.filter((item) => item.route !== '/tasks');
    }

    return baseItems;
  });
}
