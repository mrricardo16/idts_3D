export const pocStrictMissingPrefix = "/__poc_3dt__/missing/";

interface PocMiddlewareRequest {
  url?: string;
}

interface PocMiddlewareResponse {
  statusCode: number;
  setHeader: (name: string, value: string) => void;
  end: (body: string) => void;
}

export function isPocStrictMissingUrl(url: string | undefined): boolean {
  return Boolean(url?.split("?", 1)[0].startsWith(pocStrictMissingPrefix));
}

export function createPocStrict404Middleware(): (
  request: PocMiddlewareRequest,
  response: PocMiddlewareResponse,
  next: () => void,
) => void {
  return (request, response, next) => {
    if (!isPocStrictMissingUrl(request.url)) {
      next();
      return;
    }

    response.statusCode = 404;
    response.setHeader("Content-Type", "application/json; charset=utf-8");
    response.setHeader("Cache-Control", "no-store");
    response.end(JSON.stringify({ error: "POC fixture not found", path: request.url }));
  };
}
