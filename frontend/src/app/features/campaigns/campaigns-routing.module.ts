import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { CampaignsPageComponent } from './containers/campaigns-page.component';

const routes: Routes = [{ path: '', component: CampaignsPageComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CampaignsRoutingModule {}
