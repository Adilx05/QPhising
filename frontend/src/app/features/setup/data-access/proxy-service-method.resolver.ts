export const resolveProxyServiceMethod = <TArgs extends unknown[], TResult>(
  serviceType: unknown,
  methodName: string
): ((...args: TArgs) => TResult) => {
  const staticMethod = (serviceType as Record<string, unknown>)[methodName];
  if (typeof staticMethod === 'function') {
    return staticMethod as (...args: TArgs) => TResult;
  }

  if (typeof serviceType === 'function') {
    const serviceInstance = new (serviceType as new () => Record<string, unknown>)();
    const instanceMethod = serviceInstance[methodName];

    if (typeof instanceMethod === 'function') {
      return instanceMethod.bind(serviceInstance) as (...args: TArgs) => TResult;
    }
  }

  throw new Error(`Unable to resolve proxy service method '${methodName}'.`);
};
