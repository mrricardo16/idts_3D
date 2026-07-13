import { mount } from "@vue/test-utils";
import { describe, expect, it } from "vitest";
import App from "../../src/App.vue";

describe("App", () => {
  it("renders a TwinDemo stub and removes it after unmount", () => {
    const wrapper = mount(App, {
      attachTo: document.body,
      global: {
        stubs: {
          TwinDemo: {
            template: '<div data-testid="twin-demo-stub">Twin demo stub</div>',
          },
        },
      },
    });

    expect(wrapper.get('[data-testid="twin-demo-stub"]').text()).toBe("Twin demo stub");

    wrapper.unmount();

    expect(document.body.querySelector('[data-testid="twin-demo-stub"]')).toBeNull();
  });
});
