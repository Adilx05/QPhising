import { inject } from '@angular/core';
import { Router, type CanActivateFn, type CanMatchFn, type UrlTree } from '@angular/router';
import {
  allowsMainApplicationAccess,
  getRecommendedRedirectPath,
  getSetupGuardDecision
} from '../../features/setup/data-access/setup-guard.client';

const toRedirectTree = (router: Router, redirectPath: string): UrlTree =>
  router.parseUrl(redirectPath);

const resolveMainAppAccess = async (router: Router): Promise<boolean | UrlTree> => {
  const decision = await getSetupGuardDecision();

  if (allowsMainApplicationAccess(decision)) {
    return true;
  }

  return toRedirectTree(router, getRecommendedRedirectPath(decision));
};

export const setupCompletionCanActivateGuard: CanActivateFn = async () => {
  const router = inject(Router);

  return resolveMainAppAccess(router);
};

export const setupCompletionCanMatchGuard: CanMatchFn = async () => {
  const router = inject(Router);

  return resolveMainAppAccess(router);
};
