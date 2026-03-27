import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { TemplatesPageComponent } from './containers/templates-page.component';

const routes: Routes = [{ path: '', component: TemplatesPageComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TemplatesRoutingModule {}
