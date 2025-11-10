---
title: Webhooks overview
description: Clerk webhooks allow you to receive event notifications from Clerk.
lastUpdated: 2025-07-09T17:22:10.000Z
---

A webhook is an *event-driven* method of communication between applications.

Unlike typical APIs where you would need to poll for data very frequently to get it "real-time", webhooks only send data when there is an event to trigger the webhook. This makes webhooks seem "real-time", but it's important to note that they are asynchronous.

For example, if you are onboarding a new user, you can't rely on the webhook delivery as part of that flow. Typically the delivery will happen quickly, but it's not guaranteed to be delivered immediately or at all. Webhooks are best used for things like sending a notification or updating a database, but not for synchronous flows where you need to know the webhook was delivered before moving on to the next step. If you need a synchronous flow, see the <SDKLink href="/docs/references/nextjs/add-onboarding-flow" sdks={["nextjs"]}>onboarding guide</SDKLink> for an example.

## Clerk webhooks

Clerk webhooks allow you to receive event notifications from Clerk, such as when a user is created or updated. When an event occurs, Clerk will send an HTTP `POST` request to your webhook endpoint configured for the event type. The payload carries a JSON object. You can then use the information from the request's JSON payload to trigger actions in your app, such as sending a notification or updating a database.

Clerk uses [Svix](https://svix.com/) to send our webhooks.

You can find the Webhook signing secret when you select the endpoint you created on the [**Webhooks**](https://dashboard.clerk.com/last-active?path=webhooks) page in the Clerk Dashboard.

## Supported webhook events

To find a list of all the events Clerk supports:

1. In the Clerk Dashboard, navigate to the [**Webhooks**](https://dashboard.clerk.com/last-active?path=webhooks) page.
2. Select the **Event Catalog** tab.

## Payload structure

The payload of a webhook is a JSON object that contains the following properties:

* `data`: contains the actual payload sent by Clerk. The payload can be a different object depending on the `event` type. For example, for `user.*` events, the payload will always be the <SDKLink href="/docs/references/javascript/user" sdks={["js-frontend"]}>User</SDKLink> object.
* `object`: always set to `event`.
* `type`: the type of event that triggered the webhook.
* `timestamp`: timestamp in milliseconds of when the event occurred.
* `instance_id`: the identifier of your Clerk instance.

The following example shows the payload of a `user.created` event:

```json
{
  "data": {
    "birthday": "",
    "created_at": 1654012591514,
    "email_addresses": [
      {
        "email_address": "example@example.org",
        "id": "idn_29w83yL7CwVlJXylYLxcslromF1",
        "linked_to": [],
        "object": "email_address",
        "verification": {
          "status": "verified",
          "strategy": "ticket"
        }
      }
    ],
    "external_accounts": [],
    "external_id": "567772",
    "first_name": "Example",
    "gender": "",
    "id": "user_29w83sxmDNGwOuEthce5gg56FcC",
    "image_url": "https://img.clerk.com/xxxxxx",
    "last_name": "Example",
    "last_sign_in_at": 1654012591514,
    "object": "user",
    "password_enabled": true,
    "phone_numbers": [],
    "primary_email_address_id": "idn_29w83yL7CwVlJXylYLxcslromF1",
    "primary_phone_number_id": null,
    "primary_web3_wallet_id": null,
    "private_metadata": {},
    "profile_image_url": "https://www.gravatar.com/avatar?d=mp",
    "public_metadata": {},
    "two_factor_enabled": false,
    "unsafe_metadata": {},
    "updated_at": 1654012591835,
    "username": null,
    "web3_wallets": []
  },
  "instance_id": "ins_123",
  "object": "event",
  "timestamp": 1654012591835,
  "type": "user.created"
}
```

The payload should always be treated as unsafe until you validate the incoming webhook. Webhooks will originate from another server and be sent to your application as a POST request. A bad actor would fake a webhook event to try and gain access to your application or data.

## How Clerk handles delivery issues

### Retry

Svix will use a set schedule and retry any webhooks that fail. To see the up-to-date schedule, see the [Svix Retry Schedule](https://docs.svix.com/retries).

If Svix is attempting and failing to send a webhook, and that endpoint is removed or disabled from the [**Webhooks**](https://dashboard.clerk.com/last-active?path=webhooks) page in the Clerk Dashboard, then the attempts will also be disabled.

### Replay

If a webhook message or multiple webhook messages fail to send, you have the option to replay the webhook messages. This protects against your service having downtime or against a misconfigured endpoint.

To replay webhook messages:

1. In the Clerk Dashboard, navigate to the [**Webhooks**](https://dashboard.clerk.com/last-active?path=webhooks) page.
2. Select the affected endpoint.
3. In the **Message Attempts** section, next to the message you want to replay, select the menu icon on the right side, and then select **Replay**.
4. The **Replay Messages** menu will appear. You can choose to:

* Resend the specific message you selected.
* Resend all failed messages since the first failed message in that date range.
* Resend all missing messages since the first failed message in that date range.

## Sync data to your database

You can find a guide on how to use webhooks to sync your data to your database [here](/docs/webhooks/sync-data).

## Protect your webhooks from abuse

To ensure that the API route receiving the webhook can only be hit by your app, there are a few protections you should put in place:

* **Verify the request signature**: Svix webhook requests are [signed](https://www.wikiwand.com/en/Digital_signature) and can be verified to ensure the request is not malicious. To verify the signature, use Clerk's [`verifyWebhook`](https://clerk.com/docs/references/backend/verify-webhook) helper. To learn more, see Svix's guide on [how to verify webhooks with the svix libraries](https://docs.svix.com/receiving/verifying-payloads/how) or [how to verify webhooks manually](https://docs.svix.com/receiving/verifying-payloads/how-manual).

* **Only accept requests coming from [Svix's webhook IPs](https://docs.svix.com/webhook-ips.json)**:  To further prevent attackers from flooding your servers or wasting your compute, you can ensure that your webhook-receiving api routes only accept requests coming from [Svix's webhook IPs](https://docs.svix.com/webhook-ips.json), rejecting all other requests.


---
title: Debug your webhooks
description: Understand how to debug your webhooks while developing your application
lastUpdated: 2025-07-08T20:19:07.000Z
---

Developing with webhooks can be a new experience for developers. It can be hard to debug when something goes awry. This guide will cover the basics for debugging and help you to direct your attention to the correct spot.

## Webhooks and local development

When you or a user of your application performs certain actions, a webhook can be triggered. You can see the full list of [webhook events](/docs/webhooks/overview#supported-webhook-events) for a list of the actions that could result in a Webhook. Depending on the events subscribed to in the [**Webhooks**](https://dashboard.clerk.com/last-active?path=webhooks) page in the Clerk Dashboard, a webhook event will be triggered and sent to the specified endpoint in your application.

When you are developing on your localhost, your application is not internet facing and can't receive the webhook request. You will need to use a tool that creates a tunnel from the internet to your localhost. These tools will provide temporary or permanent URLs depending on the tool and the plan you subscribe to. Popular tools include `ngrok`, `localtunnel`, and `Cloudflare Tunnel`.

![Using webhooks in development](/docs/images/integrations/webhooks/webhooks_diagram.png)

Debugging webhook-related issues can be tricky, so the following sections address common issues and how to resolve them.

## Check your Middleware configuration

Incoming webhook events will never be signed in -- they are coming from a source outside of your application. Since they will be in a signed out state, the route should be public.

The following example shows the recommended Middleware configuration for your webhook routes.

```tsx {{ filename: 'middleware.tsx' }}
import { clerkMiddleware } from '@clerk/nextjs/server'

// Make sure that the `/api/webhooks/(.*)` route is not protected here
export default clerkMiddleware()

export const config = {
  matcher: [
    // Skip Next.js internals and all static files, unless found in search params
    '/((?!_next|[^?]*\\.(?:html?|css|js(?!on)|jpe?g|webp|png|gif|svg|ttf|woff2?|ico|csv|docx?|xlsx?|zip|webmanifest)).*)',
    // Always run for API routes
    '/(api|trpc)(.*)',
  ],
}
```

## Test the Route Handler or API Route

If you are having trouble with your webhook, you can create a basic Route Handler to test it locally.

1. Create a test route by adding the following file and code to your application:
   <CodeBlockTabs options={["Next.js"]}>
     ```ts {{ filename: 'app/webhooks/test/route.ts' }}
     export async function POST() {
       return Response.json({ message: 'The route is working' })
     }
     ```
   </CodeBlockTabs>
2. Run your application.
3. Send a request to the test route using the following command:
   ```bash
   curl -H 'Content-Type: application/json' \
       -X POST http://localhost:3000/api/webhooks/test
   ```

If you see the `{"message":"The route is working"}`, then the basic Route Handler is working and ready to build on.

> \[!IMPORTANT]
> Your webhook needs to return a success code like `200` or `201` when it has been successfully handled. This will mark the webhook as successful in the Dashboard and prevent [retries](/docs/webhooks/overview#retry).

## Check your configuration in the Clerk Dashboard

Whether you are developing locally or deploying to production, the webhook URL provided in your [webhook endpoint](https://dashboard.clerk.com/last-active?path=webhooks) must be exact and correct. The URL breaks down into three parts:

* the protocol (`http` vs `https`) - Whether in development using a tunnel or in production, the URL will almost always use `https` so ensure that the protocol is correct.
* the domain (`domain.com`) - The domain needs to be exact. A common error in production is not including the `www.`. Unlike entering a domain in your browser, a webhook will not be redirected from `domain.com` to `www.domain.com`. If your application lives on `www.domain.com` then the webhook URL must use that.
* the path (`/api/webhooks/user`) - The path must match the path in your application.

## Debug your tunnel and webhook delivery

If your webhook is still getting errors after testing its route locally and verifying the endpoint's configuration in the Clerk Dashboard, you can further investigate the specific errors returned by the webhook. Depending on the type of error, your approach to fixing the webhook will vary.

1. In the Clerk Dashboard, navigate to the [**Webhooks**](https://dashboard.clerk.com/last-active?path=webhooks) page.
2. Select the endpoint for which you want to test.
3. In the **Message attempts** table, you will likely see that one or more of those attempts have failed. Select the failed attempt to expand it.
4. In the details for the attempt, there will be a `HTTP RESPONSE CODE`. The code will likely be either a `500` or a `4xx` error, which will indicate there is some misconfiguration.

### Common HTTP response codes

The following table has some of the common response codes you might see and what they indicate. This is not an exhaustive list and you may need to research the code or error you are receiving. See [HTTP response status codes](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status) from MDN for reference.

| Code | Information |
| - | - |
| `400` | Usually indicates the verification failed, but could be caused by other issues. |
| `401` | The request was not authorized. If your test in the [Test the Route Handler or API Route](#test-the-route-handler-or-api-route) section worked, you should not see this error. If you are seeing it, then you will need to configure your Middleware to accept the request. |
| `404` | The URL for the webhook was not found. Check that your application is running and that [the endpoint is correct in the Clerk Dashboard](/docs/webhooks/debug-your-webhooks#check-your-configuration-in-the-clerk-dashboard). |
| `405` | Your route is not accepting a `POST` request. All webhooks are `POST` requests and the route must accept them. Unless you are using the route for something else, you can restrict the route to `POST` requests only. |
| `500` | The request made it to your application, but there is a code-related issue. This is likely a webhook verification issue or something in the webhook logic. See the following sections. |

## Debug webhook verification

To verify the webhook, see the [guide on webhooks](/docs/webhooks/sync-data) for a detailed code example. You can also visit the [Svix guide on verifying payloads](https://docs.svix.com/receiving/verifying-payloads/how).

Diagnosing a problem in this part of the webhook can be challenging. Your best bet would be the liberal use of `console.log`. You could log out the following to check if the values are correct:

* the signing secret
* the headers
* the `body` before verifying
* the result of the `.verify()` attempt

The results of these will appear in the command line where you are running your application.

Checking the values and the progress of the webhook code will allow you to narrow down where the code is failing. They will often return `null` or errors where they should be returning values.

## Check your logic

Once you have verified the webhook, you will now be writing your own code to handle the values from the webhook. This could range from saving data to a database, or integrating with another system, to updating users or sending emails or SMS.

If the webhook is verified and you're seeing a `500` status code or your webhook is not behaving as expected, remember that you can use `console.log` to help diagnose what the problem is. Console logs and errors will be displayed on your command line, allowing you to see what's happening and address the bugs.


---
title: Integrate Loops with Clerk
description: Learn how to add your Clerk users to your Loops audience, and send
  automated email sequences.
lastUpdated: 2025-07-08T20:19:07.000Z
---

<TutorialHero
  beforeYouStart={[
    {
      title: "Set up a Clerk application",
      link: "/docs/quickstarts/setup-clerk",
      icon: "clerk",
    },
    {
      title: "Set up a Loops account",
      link: "https://app.loops.so",
      icon: "cog-6-teeth",
    }
  ]}
/>

[Loops](https://loops.so) is an email platform that helps you send marketing and transactional emails to help increase revenue and engagement or improve your user's experience of your app.

This tutorial demonstrates how to sync your Clerk users to Loops so you can email users with Loop's email marketing tools. You will also learn how to send email sequences to new Clerk users.

<Steps>
  ## Create a Clerk webhook endpoint in Loops

  Loops' [Incoming webhooks](https://loops.so/docs/integrations/incoming-webhooks) feature lets Loops accept webhooks directly from external platforms like Clerk.

  To create a new webhook endpoint in Loops:

  1. Navigate to the **Clerk** settings page in the [Loops dashboard](https://app.loops.so/settings?page=clerk).
  2. Save the **Endpoint URL** somewhere secure; you're going to need it for Clerk's webhook configuration.

  ## Create a webhook in the Clerk Dashboard

  You must create a new Clerk webhook endpoint so that it can send data to Loops. Here is where you will provide the **Endpoint URL** from the last step, and then choose the events you want to listen to.

  1. In the Clerk Dashboard, navigate to the [**Webhooks**](https://dashboard.clerk.com/last-active?path=webhooks) page.
  2. Select the **Add Endpoint** button.
  3. In the **Endpoint URL** field, paste the **Endpoint URL** that you saved from Loops.
  4. In the **Message Filtering** section, select the `user.created` and/or `user.updated` events, depending on your desired setup.
  5. Select the **Create** button.
  6. You will be redirected to your endpoint's settings page. Leave this page open.

  ## Add your signing secret in Loops

  To keep communication secure between the two platforms:

  1. On the Clerk endpoint's settings page, copy the **Signing Secret**. It should be on the right side of the page with an eye icon next to it.
  2. Back in the Loops dashboard, add the **Signing Secret**.

  ## Configure Loops events

  The final step is to configure which events Loops should accept from Clerk and what Loops should do with the data.

  In the Loops dashboard, under **Complete Setup**, enable the events you selected in Clerk in step 2. Doing this will sync your Clerk users to Loops when they are created or updated in Clerk.

  For each event, you can optionally choose to trigger a "[loop](https://loops.so/docs/loop-builder)" when new webhooks are sent to Loops, by selecting an event in the **Trigger an event** field. This is useful, for example, if you want to send an email welcome sequence to new Clerk users.

  You can also choose to assign a user group value for each Clerk user, which will help you organize and segment contacts in Loops.
</Steps>


---
title: Sync Clerk data to your app with webhooks
description: Learn how to sync Clerk data to your app with webhooks.
lastUpdated: 2025-07-08T20:19:07.000Z
---

<TutorialHero
  beforeYouStart={[
    {
      title: "A Clerk app is required.",
      link: "/docs/quickstarts/overview",
    },
    {
      title: "A ngrok account is required.",
      link: "https://dashboard.ngrok.com/signup",
      icon: "user-circle",
    }
  ]}
/>

The recommended way to sync Clerk data to your app is through webhooks.

In this guide, you'll set up a webhook in your app to listen for the `user.created` event, create an endpoint in the Clerk Dashboard, build a handler for verifying the webhook, and test it locally using ngrok and the Clerk Dashboard.

Clerk offers many events, but three key events include:

* `user.created`: Triggers when a new user registers in the app or is created via the Clerk Dashboard or Backend API. Listening to this event allows the initial insertion of user information in your database.
* `user.updated`: Triggers when user information is updated via Clerk components, the Clerk Dashboard, or Backend API. Listening to this event keeps data synced between Clerk and your external database. It is recommended to only sync what you need to simplify this process.
* `user.deleted`: Triggers when a user deletes their account, or their account is removed via the Clerk Dashboard or Backend API. Listening to this event allows you to delete the user from your database or add a `deleted: true` flag.

These steps apply to any Clerk event. To make the setup process easier, it's recommended to keep two browser tabs open: one for your Clerk [**Webhooks**](https://dashboard.clerk.com/last-active?path=webhooks) page and one for your [ngrok dashboard](https://dashboard.ngrok.com).

<Steps>
  ## Set up ngrok

  To test a webhook locally, you need to expose your local server to the internet. This guide uses [ngrok](https://ngrok.com/) which creates a **forwarding URL** that sends the webhook payload to your local server.

  1. Navigate to the [ngrok dashboard](https://dashboard.ngrok.com) to create an account.
  2. On the ngrok dashboard homepage, follow the [setup guide](https://dashboard.ngrok.com/get-started/setup) instructions. Under **Deploy your app online**, select **Static domain**. Run the provided command, replacing the port number with your server's port. For example, if your development server runs on port 3000, the command should resemble `ngrok http --url=<YOUR_FORWARDING_URL> 3000`. This creates a free static domain and starts a tunnel.
  3. Save your **Forwarding** URL somewhere secure.

  ## Set up a webhook endpoint

  1. In the Clerk Dashboard, navigate to the [**Webhooks**](https://dashboard.clerk.com/last-active?path=webhooks) page.
  2. Select **Add Endpoint**.
  3. In the **Endpoint URL** field, paste the ngrok **Forwarding** URL you saved earlier, followed by `/api/webhooks`. This is the endpoint that Clerk uses to send the webhook payload. The full URL should resemble `https://fawn-two-nominally.ngrok-free.app/api/webhooks`.
  4. In the **Subscribe to events** section, scroll down and select `user.created`.
  5. Select **Create**. You'll be redirected to your endpoint's settings page. Keep this page open.

  ## Add your Signing Secret to `.env`

  To verify the webhook payload, you'll need your endpoint's **Signing Secret**. Since you don't want this secret exposed in your codebase, store it as an environment variable in your `.env` file during local development.

  1. On the endpoint's settings page in the Clerk Dashboard, copy the **Signing Secret**. You may need to select the eye icon to reveal the secret.
  2. In your project's root directory, open or create an `.env` file, which should already include your Clerk API keys. Assign your **Signing Secret** to `CLERK_WEBHOOK_SIGNING_SECRET`. The file should resemble:

  <If sdk="nuxt">
    > \[!IMPORTANT]
    > Prefix `CLERK_WEBHOOK_SIGNING_SECRET` with `NUXT_`.
  </If>

  ```env {{ filename: '.env' }}
  NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY={{pub_key}}
  CLERK_SECRET_KEY={{secret}}
  CLERK_WEBHOOK_SIGNING_SECRET=whsec_123
  ```

  ## Make sure the webhook route is public

  Incoming webhook events don't contain auth information. They come from an external source and aren't signed in or out, so the route must be public to allow access. If you're using `clerkMiddleware()`, ensure that the `/api/webhooks(.*)` route is set as public. For information on configuring routes, see the <SDKLink href="/docs/references/nextjs/clerk-middleware" sdks={["nextjs"]} code={true}>clerkMiddleware() guide</SDKLink>.

  ## Create a route handler to verify the webhook

  Set up a Route Handler that uses Clerk's <SDKLink href="/docs/references/backend/verify-webhook" sdks={["js-backend"]} code={true}>verifyWebhook()</SDKLink> function to verify the incoming Clerk webhook and process the payload.

  For this guide, the payload will be logged to the console. In a real app, you'd use the payload to trigger an action. For example, if listening for the `user.created` event, you might perform a database `create` or `upsert` to add the user's Clerk data to your database's user table.

  If the route handler returns a [4xx](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status#client_error_responses) or [5xx code](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status#server_error_responses), or no code at all, the webhook event will be [retried](/docs/webhooks/overview#retry). If the route handler returns a [2xx code](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status#successful_responses), the event will be marked as successful, and retries will stop.

  > \[!NOTE]
  > The following Route Handler can be used for any webhook event you choose to listen to. It is not specific to `user.created`.

  <Tabs items={["Next.js", "Astro", "Express", "Fastify", "Nuxt", "React Router", "TanStack React Start"]}>
    <Tab>
      ```ts {{ filename: 'app/api/webhooks/route.ts' }}
      import { verifyWebhook } from '@clerk/nextjs/webhooks'
      import { NextRequest } from 'next/server'

      export async function POST(req: NextRequest) {
        try {
          const evt = await verifyWebhook(req)

          // Do something with payload
          // For this guide, log payload to console
          const { id } = evt.data
          const eventType = evt.type
          console.log(`Received webhook with ID ${id} and event type of ${eventType}`)
          console.log('Webhook payload:', evt.data)

          return new Response('Webhook received', { status: 200 })
        } catch (err) {
          console.error('Error verifying webhook:', err)
          return new Response('Error verifying webhook', { status: 400 })
        }
      }
      ```
    </Tab>

    <Tab>
      ```ts {{ filename: 'src/pages/api/webhooks.ts' }}
      import { verifyWebhook } from '@clerk/astro/webhooks'
      import type { APIRoute } from 'astro'

      export const POST: APIRoute = async ({ request }) => {
        try {
          const evt = await verifyWebhook(request, {
            signingSecret: import.meta.env.CLERK_WEBHOOK_SIGNING_SECRET,
          })

          // Do something with payload
          // For this guide, log payload to console
          const { id } = evt.data
          const eventType = evt.type
          console.log(`Received webhook with ID ${id} and event type of ${eventType}`)
          console.log('Webhook payload:', evt.data)

          return new Response('Webhook received', { status: 200 })
        } catch (err) {
          console.error('Error verifying webhook:', err)
          return new Response('Error verifying webhook', { status: 400 })
        }
      }
      ```
    </Tab>

    <Tab>
      ```ts {{ filename: 'index.ts' }}
      import { verifyWebhook } from '@clerk/express/webhooks'
      import express from 'express'

      const app = express()

      app.post('/api/webhooks', express.raw({ type: 'application/json' }), async (req, res) => {
        try {
          const evt = await verifyWebhook(req)

          // Do something with payload
          // For this guide, log payload to console
          const { id } = evt.data
          const eventType = evt.type
          console.log(`Received webhook with ID ${id} and event type of ${eventType}`)
          console.log('Webhook payload:', evt.data)

          return res.send('Webhook received')
        } catch (err) {
          console.error('Error verifying webhook:', err)
          return res.status(400).send('Error verifying webhook')
        }
      })
      ```
    </Tab>

    <Tab>
      ```ts {{ filename: 'index.ts' }}
      import { verifyWebhook } from '@clerk/fastify/webhooks'
      import Fastify from 'fastify'

      const fastify = Fastify()

      fastify.post('/api/webhooks', async (request, reply) => {
        try {
          const evt = await verifyWebhook(request)

          // Do something with payload
          // For this guide, log payload to console
          const { id } = evt.data
          const eventType = evt.type
          console.log(`Received webhook with ID ${id} and event type of ${eventType}`)
          console.log('Webhook payload:', evt.data)

          return 'Webhook received'
        } catch (err) {
          console.error('Error verifying webhook:', err)
          return reply.code(400).send('Error verifying webhook')
        }
      })
      ```
    </Tab>

    <Tab>
      First, configure Vite to allow the ngrok host in your `nuxt.config.ts`. You only need to do this in development when tunneling your local server (e.g. `localhost:3000/api/webhooks`) to a public URL (e.g. `https://fawn-two-nominally.ngrok-free.app/api/webhooks`). In production, you won't need this configuration because your webhook endpoint will be hosted on your app's production domain (e.g. `https://your-app.com/api/webhooks`).

      ```ts {{ filename: 'nuxt.config.ts' }}
      export default defineNuxtConfig({
        // ... other config
        vite: {
          server: {
            // Replace with your ngrok host
            allowedHosts: ['fawn-two-nominally.ngrok-free.app'],
          },
        },
      })
      ```

      Then create your webhook handler:

      ```ts {{ filename: 'server/api/webhooks.post.ts' }}
      import { verifyWebhook } from '@clerk/nuxt/webhooks'

      export default defineEventHandler(async (event) => {
        try {
          const evt = await verifyWebhook(event)

          // Do something with payload
          // For this guide, log payload to console
          const { id } = evt.data
          const eventType = evt.type
          console.log(`Received webhook with ID ${id} and event type of ${eventType}`)
          console.log('Webhook payload:', evt.data)

          return 'Webhook received'
        } catch (err) {
          console.error('Error verifying webhook:', err)
          setResponseStatus(event, 400)
          return 'Error verifying webhook'
        }
      })
      ```
    </Tab>

    <Tab>
      First, configure Vite to allow the ngrok host in your `vite.config.ts`. You only need to do this in development when tunneling your local server (e.g. `localhost:3000/api/webhooks`) to a public URL (e.g. `https://fawn-two-nominally.ngrok-free.app/api/webhooks`). In production, you won't need this configuration because your webhook endpoint will be hosted on your app's production domain (e.g. `https://your-app.com/api/webhooks`).

      ```ts {{ filename: 'vite.config.ts' }}
      export default defineConfig({
        // ... other config
        server: {
          // Replace with your ngrok host
          allowedHosts: ['fawn-two-nominally.ngrok-free.app'],
        },
      })
      ```

      Then create your webhook handler:

      ```ts {{ filename: 'app/routes/webhooks.ts' }}
      import { verifyWebhook } from '@clerk/react-router/webhooks'
      import type { Route } from './+types/webhooks'

      export const action = async ({ request }: Route.ActionArgs) => {
        try {
          const evt = await verifyWebhook(request)

          // Do something with payload
          // For this guide, log payload to console
          const { id } = evt.data
          const eventType = evt.type
          console.log(`Received webhook with ID ${id} and event type of ${eventType}`)
          console.log('Webhook payload:', evt.data)

          return new Response('Webhook received', { status: 200 })
        } catch (err) {
          console.error('Error verifying webhook:', err)
          return new Response('Error verifying webhook', { status: 400 })
        }
      }
      ```

      Don't forget to add the route to your `router.ts` file:

      ```ts {{ filename: 'router.ts', mark: [5] }}
      import { type RouteConfig, route, index } from '@react-router/dev/routes'

      export default [
        index('routes/home.tsx'),
        route('api/webhooks', 'routes/webhooks.ts'),
      ] satisfies RouteConfig
      ```
    </Tab>

    <Tab>
      First, configure Vite to allow the ngrok host in your `app.config.ts`. You only need to do this in development when tunneling your local server (e.g. `localhost:3000/api/webhooks`) to a public URL (e.g. `https://fawn-two-nominally.ngrok-free.app/api/webhooks`). In production, you won't need this configuration because your webhook endpoint will be hosted on your app's production domain (e.g. `https://your-app.com/api/webhooks`).

      ```ts {{ filename: 'app.config.ts' }}
      import { defineConfig } from 'vite'

      export default defineConfig({
        server: {
          // Replace with your ngrok host
          allowedHosts: ['fawn-two-nominally.ngrok-free.app'],
        },
      })
      ```

      Then create your webhook handler:

      ```ts {{ filename: 'app/routes/api/webhooks.ts' }}
      import { verifyWebhook } from '@clerk/tanstack-react-start/webhooks'
      import { createServerFileRoute } from '@tanstack/react-start/server'

      export const ServerRoute = createServerFileRoute().methods({
        POST: async ({ request }) => {
          try {
            const evt = await verifyWebhook(request)

            // Do something with payload
            // For this guide, log payload to console
            const { id } = evt.data
            const eventType = evt.type
            console.log(`Received webhook with ID ${id} and event type of ${eventType}`)
            console.log('Webhook payload:', evt.data)

            return new Response('Webhook received', { status: 200 })
          } catch (err) {
            console.error('Error verifying webhook:', err)
            return new Response('Error verifying webhook', { status: 400 })
          }
        },
      })
      ```
    </Tab>
  </Tabs>

  ## Narrow to a webhook event for type inference

  `WebhookEvent` encompasses all possible webhook types. Narrow down the event type for accurate typing for specific events.

  In the following example, the `if` statement narrows the type to `user.created`, enabling type-safe access to evt.data with autocompletion.

  ```ts {{ filename: 'app/api/webhooks/route.ts', del: [1, 2], ins: [[4, 6]] }}
  console.log(`Received webhook with ID ${id} and event type of ${eventType}`)
  console.log('Webhook payload:', body)

  if (evt.type === 'user.created') {
    console.log('userId:', evt.data.id)
  }
  ```

  To handle types manually, import the following types from your backend SDK (e.g., `@clerk/nextjs/webhooks`):

  * `DeletedObjectJSON`
  * `EmailJSON`
  * `OrganizationInvitationJSON`
  * `OrganizationJSON`
  * `OrganizationMembershipJSON`
  * `SessionJSON`
  * `SMSMessageJSON`
  * `UserJSON`

  ## Test the webhook

  1. Start your Next.js server.
  2. In your endpoint's settings page in the Clerk Dashboard, select the **Testing** tab.
  3. In the **Select event** dropdown, select `user.created`.
  4. Select **Send Example**.
  5. In the **Message Attempts** section, confirm that the event's **Status** is labeled with **Succeeded**. In your server's terminal where your app is running, you should see the webhook's payload.

  ### Handling failed messages

  1. In the **Message Attempts** section, select the event whose **Status** is labeled with **Failed**.
  2. Scroll down to the **Webhook Attempts** section.
  3. Toggle the arrow next to the **Status** column.
  4. Review the error. Solutions vary by error type. For more information, refer to the [guide on debugging your webhooks](/docs/webhooks/debug-your-webhooks).

  ## Trigger the webhook

  To trigger the `user.created` event, create a new user in your app.

  In the terminal where your app is running, you should see the webhook's payload logged. You can also check the Clerk Dashboard to see the webhook attempt, the same way you did when [testing the webhook](#test-the-webhook).
</Steps>

## Configure your production instance

1. When you're ready to deploy your app to production, follow [the guide on deploying your Clerk app to production](/docs/deployments/overview).
2. Create your production webhook by following the steps in the previous [Set up a webhook endpoint](#set-up-a-webhook-endpoint) section. In the **Endpoint URL** field, instead of pasting the ngrok URL, paste your production app URL.
3. After you've set up your webhook endpoint, you'll be redirected to your endpoint's settings page. Copy the **Signing Secret**.
4. On your hosting platform, update your environment variables on your hosting platform by adding **Signing Secret** with the key of `CLERK_WEBHOOK_SIGNING_SECRET`.
5. Redeploy your app.
