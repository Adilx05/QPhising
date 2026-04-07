import { Component, OnInit, computed, inject, signal } from '@angular/core';

import { AppStateStore } from '../../../core/state/app-state.store';

@Component({
  standalone: false,
  selector: 'app-tasks-page',
  templateUrl: './tasks-page.component.html'
})
export class TasksPageComponent implements OnInit {
  private readonly appStateStore = inject(AppStateStore);

  protected readonly viewState = computed(() => this.appStateStore.featureViewState().tasks);
  protected readonly tasks = signal<Array<Record<string, string>>>([]);

  ngOnInit(): void {
    this.appStateStore.setFeatureError(
      'tasks',
      'Tasks list endpoint backendde henüz yok. Endpoint açılınca bu ekran doğrudan backendden beslenecek.'
    );
  }
}
