import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useController, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { apiClient } from '../../api/client';
import { ApplicationFormSchema, ApplicationFormData } from '../../utils/formValidation';
import { useEntity } from '../../hooks/useEntity';
import { EntityFormTemplate } from '../../components/forms/EntityFormTemplate';
import { FormFieldWrapper } from '../../components/forms/FormFieldWrapper';
import { DynamicFieldArray } from '../../components/forms/DynamicFieldArray';
import { DiscardChangesModal } from '../../components/forms/DiscardChangesModal';
import { hasUnsavedChanges, deepClone } from '../../utils/formHelpers';
import { updateApplicationWithCommands } from '../../utils/commandDispatcher';
import './EntityFormPage.css';

interface ApplicationFormPageProps {
  isEdit?: boolean;
}

/**
 * Application Create/Edit Form Page
 * Handles both creating new applications and editing existing ones
 */
export function ApplicationFormPage({ isEdit = false }: ApplicationFormPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [showDiscardModal, setShowDiscardModal] = useState(false);
  const [originalValues, setOriginalValues] = useState<ApplicationFormData | null>(null);

  // Fetch existing application if editing
  const { data: existingApp, isLoading: isLoadingApp } = useEntity(
    isEdit ? 'applications' : '',
    isEdit ? id : ''
  );

  const {
    register,
    control,
    watch,
    handleSubmit: handleReactFormSubmit,
    formState: { errors, isSubmitting },
    setError,
    reset,
  } = useForm<ApplicationFormData>({
    resolver: zodResolver(ApplicationFormSchema),
    defaultValues: isEdit
      ? existingApp || {}
      : {
          name: '',
          description: '',
          owner: '',
          status: 'Active',
          environment: 'Development',
          type: '',
          technologyStack: [],
          department: '',
          businessOwner: '',
          critical: false,
          url: '',
        },
  });

  const { field: technologyStackField } = useController({
    control,
    name: 'technologyStack',
  });

  const watchedValues = watch();
  const hasChanges = originalValues ? hasUnsavedChanges(originalValues, watchedValues) : false;

  // Update form when existing app loads
  useEffect(() => {
    if (existingApp) {
      reset(existingApp);
      setOriginalValues(deepClone(existingApp as ApplicationFormData));
    }
  }, [existingApp, reset]);

  useEffect(() => {
    if (!isEdit) {
      const defaults: ApplicationFormData = {
        name: '',
        description: '',
        owner: '',
        status: 'Active',
        environment: 'Development',
        type: '',
        technologyStack: [],
        department: '',
        businessOwner: '',
        critical: false,
        url: '',
      };
      setOriginalValues(defaults);
    }
  }, [isEdit]);

  const onSubmit = async (data: ApplicationFormData) => {
    try {
      let response;

      if (isEdit && id) {
        const result = await updateApplicationWithCommands(
          id,
          originalValues || {},
          data
        );
        if (result?.id) {
          navigate(`/entities/applications/${result.id}`);
        }
      } else {
        response = await apiClient.post('/applications', data);
        if (response.data?.id) {
          navigate(`/entities/applications/${response.data.id}`);
        }
      }
    } catch (error: any) {
      const errorMessage =
        error?.response?.data?.detail || error?.message || 'Failed to save application';

      if (error?.response?.status === 422 && error?.response?.data?.errors) {
        error.response.data.errors.forEach((err: any) => {
          if (err.field) {
            setError(err.field as any, {
              type: 'server',
              message: err.message,
            });
          }
        });
      }
    }
  };

  const handleCancel = () => {
    if (hasChanges) {
      setShowDiscardModal(true);
    } else {
      navigate('/entities/applications');
    }
  };

  const handleConfirmDiscard = () => {
    setShowDiscardModal(false);
    navigate('/entities/applications');
  };

  const handleSaveAndCreateAnother = async () => {
    // This will be handled by custom logic after successful submit
    // For now, submit the form
    handleReactFormSubmit(onSubmit)();
  };

  if (isEdit && isLoadingApp) {
    return <div className="entity-form-loading">Loading...</div>;
  }

  return (
    <>
      <EntityFormTemplate
        title={isEdit ? 'Edit Application' : 'Create Application'}
        subtitle={
          isEdit
            ? `Editing: ${existingApp?.name || ''}`
            : 'Add a new application to the system'
        }
        isEdit={isEdit}
        isLoading={isSubmitting}
        error={null}
        onSubmit={handleReactFormSubmit(onSubmit)}
        onCancel={handleCancel}
        submitButtonLabel={isEdit ? 'Save Changes' : 'Create Application'}
        submitButtonSecondary={
          !isEdit
            ? {
                label: 'Save & Create Another',
                onClick: handleSaveAndCreateAnother,
              }
            : undefined
        }
      >
        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Basic Information</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Application Name"
              required
              error={errors.name?.message}
              htmlFor="name"
            >
              <input
                id="name"
                type="text"
                placeholder="e.g., Customer Portal"
                {...register('name')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Type"
              required
              error={errors.type?.message}
              htmlFor="type"
            >
              <select id="type" {...register('type')}>
                <option value="">Select a type</option>
                <option value="Web">Web</option>
                <option value="Mobile">Mobile</option>
                <option value="Desktop">Desktop</option>
                <option value="API">API</option>
                <option value="Batch">Batch</option>
                <option value="Other">Other</option>
              </select>
            </FormFieldWrapper>
          </div>

          <FormFieldWrapper
            label="Description"
            error={errors.description?.message}
            htmlFor="description"
          >
            <textarea
              id="description"
              placeholder="Describe the application's purpose and functionality..."
              {...register('description')}
            />
          </FormFieldWrapper>
        </div>

        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Configuration</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Status"
              required
              error={errors.status?.message}
              htmlFor="status"
            >
              <select id="status" {...register('status')}>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
                <option value="Deprecated">Deprecated</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Environment"
              required
              error={errors.environment?.message}
              htmlFor="environment"
            >
              <select id="environment" {...register('environment')}>
                <option value="Production">Production</option>
                <option value="Staging">Staging</option>
                <option value="Development">Development</option>
                <option value="Test">Test</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Owner"
              required
              error={errors.owner?.message}
              htmlFor="owner"
            >
              <input
                id="owner"
                type="text"
                placeholder="Enter owner user ID"
                {...register('owner')}
              />
            </FormFieldWrapper>
          </div>

          <FormFieldWrapper
            label="URL"
            error={errors.url?.message}
            helpText="Include https:// or http://"
            htmlFor="url"
          >
            <input
              id="url"
              type="text"
              placeholder="https://example.com"
              {...register('url')}
            />
          </FormFieldWrapper>
        </div>

        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Advanced Information</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Department"
              error={errors.department?.message}
              htmlFor="department"
            >
              <input
                id="department"
                type="text"
                placeholder="e.g., Engineering"
                {...register('department')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Business Owner"
              error={errors.businessOwner?.message}
              htmlFor="businessOwner"
            >
              <input
                id="businessOwner"
                type="text"
                placeholder="Enter business owner user ID"
                {...register('businessOwner')}
              />
            </FormFieldWrapper>
          </div>

          <FormFieldWrapper
            label="Critical Application"
            error={errors.critical?.message}
          >
            <label className="checkbox-label">
              <input type="checkbox" {...register('critical')} />
              <span>This is a critical application</span>
            </label>
          </FormFieldWrapper>

          <DynamicFieldArray
            label="Technology Stack"
            value={technologyStackField.value || []}
            onChange={technologyStackField.onChange}
            placeholder="e.g., React, Node.js, PostgreSQL"
            error={errors.technologyStack?.message}
          />
        </div>
      </EntityFormTemplate>

      <DiscardChangesModal
        isOpen={showDiscardModal}
        onConfirm={handleConfirmDiscard}
        onCancel={() => setShowDiscardModal(false)}
      />
    </>
  );
}
