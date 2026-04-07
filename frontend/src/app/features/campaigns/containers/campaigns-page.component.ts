import { Component, OnInit, computed, inject, signal } from '@angular/core';

import { AppApiFacade } from '../../../core/api/app-api.facade';
import { AppStateStore } from '../../../core/state/app-state.store';

@Component({
  standalone: false,
  selector: 'app-campaigns-page',
  templateUrl: './campaigns-page.component.html'
})
export class CampaignsPageComponent implements OnInit {
  private readonly appStateStore = inject(AppStateStore);
  private readonly appApiFacade = inject(AppApiFacade);

  protected readonly viewState = computed(() => this.appStateStore.featureViewState().campaigns);
  protected readonly campaigns = signal<Array<Record<string, string>>>([]);

  ngOnInit(): void {
    void this.loadCampaigns();
  }

  private async loadCampaigns(): Promise<void> {
    this.appStateStore.setFeatureLoading('campaigns', true);
    this.appStateStore.setFeatureError('campaigns', null);

    try {
      this.campaigns.set(await this.appApiFacade.listCampaignRows());
    } catch (error) {
      console.error('Failed to load campaigns', error);
      this.appStateStore.setFeatureError('campaigns', 'Campaign listesi backendden alınamadı.');
      this.campaigns.set([]);
    } finally {
      this.appStateStore.setFeatureLoading('campaigns', false);
    }
  }
}
