import { Component, OnInit, computed, inject, signal } from '@angular/core';

import { AppStateStore } from '../../../core/state/app-state.store';

@Component({
  standalone: false,
  selector: 'app-tracking-page',
  templateUrl: './tracking-page.component.html'
})
export class TrackingPageComponent implements OnInit {
  private readonly appStateStore = inject(AppStateStore);

  protected readonly viewState = computed(() => this.appStateStore.featureViewState().tracking);
  protected readonly events = signal<Array<Record<string, string>>>([]);

  ngOnInit(): void {
    this.appStateStore.setFeatureError(
      'tracking',
      'Tracking list endpoint backendde henüz yok. Bu ekran canlı endpoint eklendiğinde otomatik bağlanacak.'
    );
  }
}
