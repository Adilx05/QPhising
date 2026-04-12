import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Route, Router, UrlSegment, UrlTree } from '@angular/router';

import { SetupStateService } from './setup-state.service';

async function evaluateAppRoute(): Promise<true | UrlTree> {
  const setupStateService = inject(SetupStateService);
  const router = inject(Router);

  const isCompleted = await setupStateService.isSetupCompleted();
  return isCompleted ? true : router.createUrlTree(['/setup']);
}

async function evaluateSetupRoute(): Promise<true | UrlTree> {
  const setupStateService = inject(SetupStateService);
  const router = inject(Router);

  const isCompleted = await setupStateService.isSetupCompleted();
  return isCompleted ? router.createUrlTree(['/dashboard']) : true;
}

export const setupCompletionGuard: CanActivateFn = async () => evaluateAppRoute();

export const setupCompletionMatchGuard: CanMatchFn = async (_route: Route, _segments: UrlSegment[]) => evaluateAppRoute();

export const setupPageGuard: CanActivateFn = async () => evaluateSetupRoute();

export const setupPageMatchGuard: CanMatchFn = async (_route: Route, _segments: UrlSegment[]) => evaluateSetupRoute();
